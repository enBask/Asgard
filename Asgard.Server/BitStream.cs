using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard
{
    public sealed class BitStream
    {
        byte[] _rawData;

        public byte[] GetData()
        {
            return _rawData;
        }

        public void SetData(byte[] data)
        {
            _rawData = data;
        }

        public string GetString()
        {
            return null;
        }

        public void SetString(string data)
        {

        }
    }
}
