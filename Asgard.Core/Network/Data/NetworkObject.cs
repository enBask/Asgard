using Artemis;
using Artemis.Interface;
using Asgard.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Data
{
    public class NetworkObject : IComponent
    {
        private bool _owned = false;
        private int _channel = 0;

        public NetworkObject()
        {
        }

        public NetworkObject(int channel) : this()
        {
            _channel = channel;
        }

        internal void SetOwned()
        {
            _owned = true;
        }
    }

    public class StateSyncNetworkObject : NetworkObject
    {
        public StateSyncNetworkObject() : base()
        {
        }

        public StateSyncNetworkObject(int channel) : base(channel)
        {

        }
    }

    public class UnreliableStateSyncNetworkObject : StateSyncNetworkObject
    {
        public UnreliableStateSyncNetworkObject() : base()
        {
        }

        public UnreliableStateSyncNetworkObject(int channel) : base(channel)
        {

        }
    }

    public class DefinitionNetworkObject: NetworkObject
    {
        public DefinitionNetworkObject() : base()
        {
        }

        public DefinitionNetworkObject(int channel) : base(channel)
        {

        }

        public virtual void OnCreated(AsgardBase instance, Entity entity)
        {

        }
    }
    
    public class NetworkProperty<T>
    {
        private T _value;
        private bool _changed;
        private bool _set;

        public bool HasChanged { get { return _changed; } }
        internal void ClearChange() { _changed = false; }

        public T Value
        {
            get
            {
                if (!_set)
                {
                    _changed = true;
                    _value = default(T);
                }

                return _value;
            }
            set
            {
                _value = value;
                _changed = true;
                _set = true;
            }
        }

        public static implicit operator NetworkProperty<T>(T data)
        {
            NetworkProperty<T> p = new NetworkProperty<T>();
            p.Value = data;
            return p;
        }
        public static implicit operator T (NetworkProperty<T> data)
        {
            if (data == null)
                return default(T);

            return data.Value;
        }

        public override string ToString()
        {
            if (_set)
                return Value.ToString();
            else
                return null;
        }

        public override int GetHashCode()
        {
            if (_set)
                return Value.GetHashCode();
            return
                0;
        }
    }
}
