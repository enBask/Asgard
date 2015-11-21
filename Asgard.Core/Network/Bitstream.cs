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

        public ushort ReadUInt16()
        {
            return _buffer.ReadUInt16();
        }

        public void Write(ushort data)
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

    }
}
