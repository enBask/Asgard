using Asgard.Core.Interpolation;
using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Client.Collections
{
    public static class MathHelpers
    {
        public static float LinearInterpolate(float a, float b, float t)
        {
            var r =  a + (b - a) * t;
            return r;
        }
    }    

    public class InterpolationBuffer<T, TData>
        where TData : class
        where T : Packet, IInterpolationPacket<TData>
    {
        private double _tickRate = 0;
        private double _delay = 0;
        private int _frameCount = 0;
        private CircularBuffer<T> _buffer = new CircularBuffer<T>(60);

        private static List<PropertyInfo> _properties = new List<PropertyInfo>();
        private static List<FieldInfo> _fields = new List<FieldInfo>();

        double start_time = 0;
        bool started = false;

        uint start_id;
        uint end_id;
        double interpolation_start_time;
        double interpolation_end_time;
        bool is_interpolating = false;

        static InterpolationBuffer()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var type = typeof(TData);
            var props = type.GetProperties(flags);
            foreach(var prop in props)
            {
                if (prop.GetCustomAttribute<InterpolationAttribute>() != null)
                {
                    _properties.Add(prop);
                }
            }

            var fields = type.GetFields(flags);
            foreach(var field in fields)
            {
                if (field.GetCustomAttribute<InterpolationAttribute>() != null)
                {
                    _fields.Add(field);
                }
            }
        }

        public InterpolationBuffer(double tickRate, double delay)
        {
            _tickRate = tickRate;
            _delay = delay;
            _frameCount = (int)Math.Floor(_delay / (1.0 / _tickRate));
        }

        public void Add(T data)
        {
            _buffer.Add(data.Id, data);
            started = true;
        }

        public T Get(uint index)
        {
            var snap = _buffer.Get(index);
            return snap;
        }

        public List<TData> Update(double time)
        {
            if (!started)
            {
                return null;
            }

            time -= (_delay);
            if (time <= 0)
            {
                return null;
            }

            uint id = (uint)Math.Floor( time * _tickRate);


            double id_from_start = time * _tickRate;

            if (is_interpolating)
            {
                uint id_check = (uint)Math.Floor(id_from_start);
                long diff = Math.Abs(id_check - start_id);
                if (diff > _frameCount)
                {
                    is_interpolating = false;
                }
            }


            if (!is_interpolating)
            {
                uint interpolation_id = (uint)Math.Floor(id_from_start);

                var snapshot = _buffer.Get(interpolation_id);
                if (snapshot != null)
                {
                    start_id = interpolation_id;
                    end_id = start_id;

                    interpolation_start_time = id_from_start * (1.0f / _tickRate);
                    interpolation_end_time = interpolation_start_time;
                    is_interpolating = true;
                }
            }

            if (!is_interpolating)
            {
                return null;
            }

            if (time < interpolation_start_time)
                time = interpolation_start_time;

            if (time >= interpolation_end_time)
            {
                start_id = end_id;
                interpolation_start_time = interpolation_end_time;

                for(uint i =1; i <= _frameCount; ++i)
                {
                    var next_snap = _buffer.Get(start_id + i);
                    if (next_snap != null)
                    {
                        end_id = start_id + i;
                        interpolation_end_time = interpolation_start_time + (1.0f / _tickRate) * i;
                        break;
                    }
                }
            }

            if (time >= interpolation_end_time)
            {
                return null;
            }

            var snap_a = _buffer.Get(start_id);
            var snap_b = _buffer.Get(end_id);

            if (snap_a == null || snap_b == null)
            {
                return null;
            }

            float t = (float)((time - interpolation_start_time) / (interpolation_end_time - interpolation_start_time));
            t = Math.Min(t, 1.0f);
            t = Math.Max(t, 0.0f);


            List<TData> retList = new List<TData>();
            for(int i =0; i < snap_a.DataPoints.Count; ++i)
            {
                var obj_a = snap_a.DataPoints[i];
                var obj_b = snap_b.DataPoints[i];

                TData interp_object = Activator.CreateInstance<TData>();
                foreach (var prop in _properties)
                {
                    prop.SetValue(interp_object,
                        MathHelpers.LinearInterpolate
                        (
                            (float)prop.GetValue(obj_a),
                            (float)prop.GetValue(obj_b),
                            t)
                        );
                }

                foreach (var field in _fields)
                {
                    field.SetValue(interp_object,
                        MathHelpers.LinearInterpolate
                        (
                            (float)field.GetValue(obj_a),
                            (float)field.GetValue(obj_b),
                            t)
                        );
                }

                retList.Add(interp_object);

            }

            return retList;

        }
    }
}
