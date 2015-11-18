using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Network
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
    }
}
