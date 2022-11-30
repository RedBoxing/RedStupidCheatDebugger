using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStupidCheatDebugger
{
    public class Buffer
    {
        public byte[] _buffer;

        public int readOffset
        {
            get; set;
        }
        
        public int writeOffset
        {
            get; set;
        }
        

        public Buffer(int size)
        {
            _buffer = new byte[size];
            readOffset = 0;
            writeOffset = 0;
        }

        public void Resize(int newSize)
        {
            byte[] newBuffer = new byte[newSize];
            Array.Copy(_buffer, newBuffer, _buffer.Length);
            _buffer = newBuffer;
        }

        public void Write(byte[] data, int offset)
        {
            if (offset + data.Length > _buffer.Length)
                throw new Exception("Can't write: Buffer overflow");

            Array.Copy(data, 0, _buffer, offset, data.Length);
        }

        public void Write(byte[] data)
        {
            Write(data, writeOffset);
            writeOffset += data.Length;
        }

        public byte[] Read(int offset, int size)
        {
            if (offset + size > _buffer.Length)
                throw new Exception("Can't read: Buffer overflow");

            byte[] data = new byte[size];
            Array.Copy(_buffer, offset, data, 0, size);

            return data;
        }

        public byte[] Read(int size)
        {
            byte[] data = Read(readOffset, size);
            readOffset += size;

            return data;
        }

        public UInt64 ReadUInt64(int offset)
        {
            return BitConverter.ToUInt64(Read(offset, 8), 0);
        }

        public UInt64 ReadUInt64()
        {
            UInt64 value = BitConverter.ToUInt64(Read(readOffset, 8), 0);
            readOffset += 8;
            return value;
        }

        public UInt32 ReadUInt32(int offset)
        {
            return BitConverter.ToUInt32(Read(offset, 4), 0);
        }

        public UInt32 ReadUInt32()
        {
            UInt32 value = BitConverter.ToUInt32(Read(readOffset, 4), 0);
            readOffset += 4;
            return value;
        }

        public UInt16 ReadUInt16(int offset)
        {
            return BitConverter.ToUInt16(Read(offset, 2), 0);
        }

        public UInt16 ReadUInt16()
        {
            UInt16 value = BitConverter.ToUInt16(Read(readOffset, 2), 0);
            readOffset += 2;
            return value;
        }

        public Int64 ReadInt64(int offset)
        {
            return BitConverter.ToInt64(Read(offset, 8), 0);
        }

        public Int64 ReadInt64()
        {
            Int64 value = BitConverter.ToInt64(Read(readOffset, 8), 0);
            readOffset += 8;
            return value;
        }

        public Int32 ReadInt32(int offset)
        {
            return BitConverter.ToInt32(Read(offset, 4), 0);
        }

        public Int32 ReadInt32()
        {
            Int32 value = BitConverter.ToInt32(Read(readOffset, 4), 0);
            readOffset += 4;
            return value;
        }

        public Int16 ReadInt16(int offset)
        {
            return BitConverter.ToInt16(Read(offset, 2), 0);
        }

        public Int16 ReadInt16()
        {
            Int16 value = BitConverter.ToInt16(Read(readOffset, 2), 0);
            readOffset += 2;
            return value;
        }

        public float ReadFloat(int offset)
        {
            return BitConverter.ToSingle(Read(offset, 4), 0);
        }

        public float ReadFloat()
        {
            float value = BitConverter.ToSingle(Read(readOffset, 4), 0);
            readOffset += 4;
            return value;
        }

        public double ReadDouble(int offset)
        {
            return BitConverter.ToDouble(Read(offset, 8), 0);
        }

        public double ReadDouble()
        {
            double value = BitConverter.ToDouble(Read(readOffset, 8), 0);
            readOffset += 8;
            return value;
        }

        public byte ReadByte(int offset)
        {
            return Read(offset, 1)[0];
        }

        public byte ReadByte()
        {
            byte value = Read(readOffset, 1)[0];
            readOffset += 1;
            return value;
        }

        public bool ReadBool(int offset)
        {
            return BitConverter.ToBoolean(Read(offset, 1), 0);
        }

        public bool ReadBool()
        {
            bool value = BitConverter.ToBoolean(Read(readOffset, 1), 0);
            readOffset += 1;
            return value;
        }

        public string ReadString(int offset, int size)
        {
            return Encoding.UTF8.GetString(Read(offset, size));
        }

        public string ReadString(int size)
        {
            string value = Encoding.UTF8.GetString(Read(readOffset, size));
            readOffset += size;
            return value;
        }

        public string ReadString()
        {
            UInt64 size = ReadUInt64();
            MessageBox.Show("Offset:" + readOffset.ToString() + ", Size:" + size.ToString() + ", Length: " + _buffer.Length.ToString());
            string value = Encoding.UTF8.GetString(Read(readOffset, (int)size));
            readOffset += (int)size + 8;
            return value;
        }

        public void WriteUInt64(int offset, UInt64 value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteUInt64(UInt64 value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 8;
        }

        public void WriteUInt32(int offset, UInt32 value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteUInt32(UInt32 value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 4;
        }

        public void WriteUInt16(int offset, UInt16 value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteUInt16(UInt16 value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 2;
        }

        public void WriteInt64(int offset, Int64 value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteInt64(Int64 value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 8;
        }

        public void WriteInt32(int offset, Int32 value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteInt32(Int32 value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 4;
        }

        public void WriteInt16(int offset, Int16 value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteInt16(Int16 value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 2;
        }

        public void WriteFloat(int offset, float value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteFloat(float value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 4;
        }

        public void WriteDouble(int offset, double value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteDouble(double value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 8;
        }

        public void WriteByte(int offset, byte value)
        {
            Write(new byte[] { value }, offset);
        }

        public void WriteByte(byte value)
        {
            Write(new byte[] { value }, writeOffset);
            writeOffset += 1;
        }

        public void WriteBool(int offset, bool value)
        {
            Write(BitConverter.GetBytes(value), offset);
        }

        public void WriteBool(bool value)
        {
            Write(BitConverter.GetBytes(value), writeOffset);
            writeOffset += 1;
        }

        public void WriteString(int offset, string value)
        {
            WriteUInt64(offset, (UInt64)value.Length);
            Write(Encoding.ASCII.GetBytes(value), offset + 8);
        }

        public void WriteString(string value)
        {
            WriteUInt64((UInt64)value.Length);
            Write(Encoding.ASCII.GetBytes(value), writeOffset);
            writeOffset += value.Length + 8;
        }
    }
}
