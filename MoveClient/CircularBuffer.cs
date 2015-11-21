using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveClient
{
    public class CircularBuffer<T> where T : class
    {
        T[] _buffer;
        Dictionary<long, uint> _lookupChecker = new Dictionary<long, uint>();
        uint _size;

        public CircularBuffer(uint size)
        {
            _size = size;
            _buffer = new T[size];
        }

        public void Add(uint lookup, T data)
        {
            var index = lookup % _size;

            _buffer[index] = data;
            _lookupChecker[index] = lookup;
        }

        public void Remove(uint lookup)
        {
            var index = lookup % _size;
            _buffer[index] = null;
            _lookupChecker[index] = UInt32.MaxValue;
        }

        public T Get(uint lookup)
        {
            var index = lookup % _size;
            var data = _buffer[index];

            if (data != null && _lookupChecker[index] == lookup)
            {
                return data;
            }

            return null;

        }
    }
}
