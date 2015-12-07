using Asgard.Core.Interpolation;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Collections
{
    public class JitterBuffer<T>
        where T : class
    {
        private Queue<List<T>> _buffer;
        bool started = false;
        float _startTime = 0f;
        float _tickRate;

        public JitterBuffer(float tickRate)
        {
            _tickRate = tickRate;
            _buffer = new Queue<List<T>>();
        }

        public void Add(List<T> data)
        {
            _buffer.Enqueue(data);
            if (_buffer.Count > _tickRate * 1 )
            {
                _buffer.Dequeue();
            }

            if (!started)
            {
                started = true;
                _startTime = NetTime.RealTime;
            }
        }

        public List<T> Get()
        {
            if (!started) return null;

            var diff = NetTime.RealTime - _startTime;
            if (diff < 0.05f) return null;

            if (_buffer.Count > 0)
                return _buffer.Dequeue();
            else
            {
                started = false;
                return null;
            }
        }     

    }
}
