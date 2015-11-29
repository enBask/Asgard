using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network
{
    public class Bitstream
    {
        NetBuffer _buffer;
        public Bitstream(NetBuffer buffer)
        {
            _buffer = buffer;
        }

        public void WriteVariableUInt32(uint data)
        {
            _buffer.WriteVariableUInt32(data);
        }

        public uint ReadVariableUInt32()
        {
            return _buffer.ReadVariableUInt32();
        }

        public void WriteVariableInt32(int data)
        {
            _buffer.WriteVariableInt32(data);
        }

        public int ReadVariableInt32()
        {
            return _buffer.ReadVariableInt32();
        }

        public void WriteVariable(long data, int numOfBits)
        {
            _buffer.Write(data, numOfBits);
        }

        public long ReadVariable(int numOfBits)
        {
            return _buffer.ReadInt64(numOfBits);
        }

        public void WriteVariable(ulong data, int numOfBits)
        {
            _buffer.Write(data, numOfBits);
        }

        public ulong ReadUVariable(int numOfBits)
        {
            return _buffer.ReadUInt64(numOfBits);
        }

        public bool ReadBool()
        {
            return _buffer.ReadBoolean();
        }

        public void Write(bool data)
        {
            _buffer.Write(data);
        }

        public string ReadString()
        {
            return _buffer.ReadString();
        }

        public void Write(string data)
        {
            _buffer.Write(data);
        }

        public int ReadInt32()
        {
            return _buffer.ReadInt32();
        }

        public void Write(int data)
        {
            _buffer.Write(data);
        }

        public uint ReadUInt32()
        {
            return _buffer.ReadUInt32();
        }

        public void Write(uint data)
        {
            _buffer.Write(data);
        }

        public ushort ReadUInt16()
        {
            return _buffer.ReadUInt16();
        }

        public void Write(ushort data)
        {
            _buffer.Write(data);
        }

        public short ReadInt16()
        {
            return _buffer.ReadInt16();
        }

        public void Write(short data)
        {
            _buffer.Write(data);
        }

        public double ReadDouble()
        {
            return _buffer.ReadDouble();
        }

        public void Write(double data)
        {
            _buffer.Write(data);
        }

        public float ReadFloat()
        {
            return _buffer.ReadFloat();
        }

        public void Write(float data)
        {
            _buffer.Write(data);
        }

        public byte ReadByte()
        {
            return _buffer.ReadByte();
        }

        public void Write(byte data)
        {
            _buffer.Write(data);
        }

        public sbyte ReadSByte()
        {
            return _buffer.ReadSByte();
        }

        public void Write(sbyte data)
        {
            _buffer.Write(data);
        }

        public ulong ReadUInt64()
        {
            return _buffer.ReadUInt64();
        }

        public void Write(ulong data)
        {
            _buffer.Write(data);
        }

        public long ReadInt64()
        {
            return _buffer.ReadInt64();
        }

        public void Write(long data)
        {
            _buffer.Write(data);
        }
    }
}
