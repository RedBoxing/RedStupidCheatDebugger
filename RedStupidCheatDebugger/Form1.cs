using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RedStupidCheatDebugger
{
    public enum Command : byte
    {
        forceOpenCheatProcess,
        readMemory,
        writeMemory,
        getTitleID,
        getBuildID,
        getHeapBaseAddress,
        getHeapSize,
        getMainNsoBaseAddress,
        getMainNsoSize,
    }

    public enum ScanDataCondition
    {
        ANY,
        CHANGED,
        NOT_CHANGED,
        EQUALS_TO,
        INCREASED,
        DECREASED
    }

    public enum MemorySection
    {
        MAIN,
        HEAP,
        ABSOLUTE
    }

    public partial class Form1 : Form
    {
        private Socket socket;
        private UInt64 mainBase;
        private UInt64 mainSize;
        private UInt64 heapBase;
        private UInt64 heapSize;

        private Dictionary<UInt64, byte[]> searchList = new Dictionary<UInt64, byte[]>();
        private bool connected = false;

        public Form1()
        {
            InitializeComponent();

            this.setEnabledControls(this.tabControl1, false);
            this.updateButtons();
        }

        private async Task connect(string ip, int port)
        {
            try {
                IPAddress iPAddress = IPAddress.Parse(ip);
                IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);

                this.socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await this.socket.ConnectAsync(iPEndPoint);

                await this.forceOpenCheatProcess();
                this.mainBase = await this.getMainBase();
                this.mainSize = await this.getMainSize();
                this.heapBase = await this.getHeapBase();
                this.heapSize = await this.getHeapSize();
                this.connected = true;

                this.textBox4.Text = this.mainBase.ToString("X");
                this.textBox5.Text = this.mainSize.ToString("X");
                this.textBox7.Text = this.heapBase.ToString("X");
                this.textBox6.Text = this.heapSize.ToString("X");

                this.setEnabledControls(this.tabControl1, true);
                this.updateButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to connect !");
            }
        }

        private async Task<bool> forceOpenCheatProcess()
        {
            await this.sendCommand(Command.forceOpenCheatProcess);
            return true;
        }

        private async Task<byte[]> readMemory(UInt64 address, UInt64 size, MemorySection memorySection)
        {
            Buffer buf = new Buffer(16 + memorySection.ToString().Length);
            buf.WriteString(memorySection.ToString());
            buf.WriteUInt64(address);
            buf.WriteUInt64(size);

            return (await this.sendCommand(Command.readMemory, buf)).Read((int)size);
        }

        private async Task writeMemory(UInt64 address, byte[] data, MemorySection memorySection)
        {
            Buffer buf = new Buffer(8 + memorySection.ToString().Length + data.Length);
            buf.WriteString(memorySection.ToString());
            buf.WriteUInt64(address);
            buf.WriteUInt64((UInt64)data.Length);
            buf.Write(data);

            await this.sendCommand(Command.writeMemory, buf);
        }

        private async Task<string> getTitleID()
        {
            Buffer buffer = await this.sendCommand(Command.getTitleID);
            UInt64 titleID = buffer.ReadUInt64();
            return titleID.ToString("X16");
        }

        private async Task<string> getBuildID()
        {
            Buffer buffer = await this.sendCommand(Command.getTitleID);
            UInt64 buildID = buffer.ReadUInt64();
            return buildID.ToString("X16");
        }

        private async Task<UInt64> getMainBase()
        {
            Buffer buffer = await this.sendCommand(Command.getMainNsoBaseAddress);
            return buffer.ReadUInt64();
        }

        private async Task<UInt64> getMainSize()
        {
            Buffer buffer = await this.sendCommand(Command.getMainNsoSize);
            return buffer.ReadUInt64();
        }

        private async Task<UInt64> getHeapBase()
        {
            Buffer buffer = await this.sendCommand(Command.getHeapBaseAddress);
            return buffer.ReadUInt64();
        }

        private async Task<UInt64> getHeapSize()
        {
            Buffer buffer = await this.sendCommand(Command.getHeapSize);
            return buffer.ReadUInt64();
        }

        private UInt64 bytesToValue(byte[] bytes, UInt64 size)
        {
            UInt64 value = 0;
            switch (size)
            {
                case 8:
                    value = (UInt64)bytes[0];
                    break;
                case 16:
                    value = (UInt64)BitConverter.ToUInt16(bytes, 0);
                    break;
                case 32:
                    value = (UInt64)BitConverter.ToUInt32(bytes, 0);
                    break;
                case 64:
                    value = (UInt64)BitConverter.ToUInt64(bytes, 0);
                    break;
            }

            return value;
        }
        
        private async Task scan(UInt64 dataSize, UInt64 start, UInt64 end, UInt64 expectedValue, ScanDataCondition condition)
        {
            this.setEnabledControls(this.tabControl1, false);
            int nbrAddressToScan = (int)((end - start) / dataSize);
            this.progressBar1.Maximum = nbrAddressToScan;

            if (searchList.Count == 0) // first scan
            {
                MessageBox.Show("First scan");
                MessageBox.Show("start: " + start.ToString("X") + ", end: " + end.ToString("X"));
                for (UInt64 i = start; i < end; i += dataSize)
                {
                    MessageBox.Show("Reading memory at " + i.ToString() + " with size " + dataSize);
                    byte[] data = await this.readMemory(i, dataSize, MemorySection.ABSOLUTE);
                    UInt64 value = bytesToValue(data, dataSize);

                    MessageBox.Show("Address: " + i.ToString("X") + " Value: " + value.ToString("X"));

                    if (condition == ScanDataCondition.ANY || (condition == ScanDataCondition.EQUALS_TO && value == expectedValue))
                    {
                        searchList.Add(i, data);
                    }

                    this.progressBar1.Value++;
                }

                MessageBox.Show("Found " + searchList.Count + " results");
            }
            else
            {
                Dictionary<UInt64, byte[]> newSearchList = new Dictionary<UInt64, byte[]>();
                foreach (KeyValuePair<UInt64, byte[]> entry in searchList)
                {
                    byte[] data = await this.readMemory(entry.Key, dataSize, MemorySection.ABSOLUTE);
                    UInt64 oldValue = bytesToValue(entry.Value, dataSize);
                    UInt64 newValue = bytesToValue(data, dataSize);

                    if ((condition == ScanDataCondition.EQUALS_TO && newValue == expectedValue)
                        || (condition == ScanDataCondition.CHANGED && newValue != oldValue)
                        || (condition == ScanDataCondition.NOT_CHANGED && newValue == oldValue)
                        || (condition == ScanDataCondition.INCREASED && newValue > oldValue)
                        || (condition == ScanDataCondition.DECREASED && newValue < oldValue))
                    {
                        newSearchList.Add(entry.Key, data);
                    }
                }

                searchList = newSearchList;
            }

            this.setEnabledControls(this.tabControl1, true);
            this.progressBar1.Style = ProgressBarStyle.Continuous;
        }

        private async Task<Buffer> sendCommand(Command command, Buffer buffer) {
            Buffer writeBuffer = new Buffer(8 + 1 + buffer._buffer.Length);
            writeBuffer.WriteUInt64(0);
            writeBuffer.WriteByte((byte)command);
            writeBuffer.Write(buffer._buffer);
            writeBuffer.WriteUInt64(0, (UInt64)writeBuffer._buffer.Length - 8);

            int written = await this.socket.SendAsync(writeBuffer._buffer, SocketFlags.None);
            if (written != writeBuffer._buffer.Length)
            {
                throw new Exception("Error sending command");
            }

            Buffer readBuffer = new Buffer(8);

            int read = this.socket.Receive(readBuffer._buffer, 8, SocketFlags.None);
            if (read != 8)
            {
                throw new Exception("Error reading command response");
            }

            UInt64 responseLength = readBuffer.ReadUInt64();
            readBuffer = new Buffer((int)responseLength);
            read = this.socket.Receive(readBuffer._buffer, (int)responseLength, SocketFlags.None);
            if (read != (int)responseLength)
            {
                throw new Exception("Error reading command response");
            }

            if (!readBuffer.ReadBool())
            {
                throw new Exception(readBuffer.ReadString());
            }

            return readBuffer;
        }

        private async Task<Buffer> sendCommand(Command command)
        {
            return await this.sendCommand(command, new Buffer(0));
        }
      
        private void button1_Click(object sender, EventArgs e)
        {
            this.connect("192.168.1.92", 6060);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UInt64 dataSize;
            UInt64 start;
            UInt64 end;
            UInt64 expectedValue = UInt64.Parse(this.textBox3.Text);
            ScanDataCondition condition;

            switch (this.comboBox1.SelectedIndex)
            {
                case 0:
                    dataSize = 8;
                    break;
                case 1:
                    dataSize = 16;
                    break;
                case 2:
                    dataSize = 32;
                    break;
                case 3:
                    dataSize = 64;
                    break;
                default:
                    dataSize = 32;
                    break;
            }

            switch (this.comboBox2.SelectedIndex)
            {
                case 0:
                    MessageBox.Show("Not supported yet!");
                    return;
                case 1:
                    start = this.mainBase;
                    end = this.mainBase + this.mainSize;
                    break;
                case 2:
                    start = this.heapBase;
                    end = this.heapBase + this.heapSize;
                    break;
                default:
                    start = this.mainBase;
                    end = this.mainBase + this.mainSize;
                    break;
            }

            if (this.comboBox3.SelectedIndex == 0)
            {
                switch (this.comboBox4.SelectedIndex)
                {
                    case 0:
                        condition = ScanDataCondition.CHANGED;
                        break;
                    case 1:
                        condition = ScanDataCondition.NOT_CHANGED;
                        break;
                    case 2:
                        condition = ScanDataCondition.EQUALS_TO;
                        break;
                    case 3:
                        condition = ScanDataCondition.INCREASED;
                        break;
                    case 4:
                        condition = ScanDataCondition.DECREASED;
                        break;
                    default:
                        condition = ScanDataCondition.NOT_CHANGED;
                        break;
                }
            }
            else
            {
                condition = ScanDataCondition.ANY;
            }

            this.scan(dataSize, start, end, expectedValue, condition).ContinueWith((task) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.dataGridView1.Rows.Clear();
                    foreach (KeyValuePair<UInt64, byte[]> entry in searchList)
                    {
                        this.dataGridView1.Rows.Add(entry.Key.ToString("X"), BitConverter.ToUInt64(entry.Value, 0).ToString());
                    }
                });
            });
        }

        private void setEnabledControls(Control control, bool enabled)
        {
            foreach (Control c in control.Controls)
            {
                setEnabledControls(c, enabled);
            }
            control.Enabled = enabled;
        }

        private void updateButtons()
        {
            if (this.connected)
            {
                this.button1.Enabled = false;
                this.button3.Enabled = true;
                this.textBox1.Enabled = false;
                this.textBox2.Enabled = false;
            }
            else
            {
                this.button1.Enabled = true;
                this.button3.Enabled = false;
                this.textBox1.Enabled = true;
                this.textBox2.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.socket.Close();
            this.updateButtons();
        }
    }
}