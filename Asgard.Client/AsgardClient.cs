using Artemis;
using Artemis.Interface;
using Artemis.Manager;
using Asgard.Client.Collections;
using Asgard.Client.Network;
using Asgard.Core.Collections;
using Asgard.Core.Interpolation;
using Asgard.Core.Network;
using Asgard.Core.Network.Data;
using Asgard.Core.Network.Packets;
using Asgard.Core.System;
using Asgard.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Client
{
    public class AsgardClient<TSnapshotPacket, TData, TClientData> :  AsgardBase
        where TData : class
        where TClientData: Packet
        where TSnapshotPacket: Packet, IInterpolationPacket<TData>
    {

        protected bool _pumpNetwork = true;

        public delegate void AsgardClientCallback();
        public delegate void SnapshotCallback(TSnapshotPacket snapshot);

        public event SnapshotCallback OnSnapshot;

        JitterBuffer<Tuple<Entity,NetworkObject>> _jitterBuffer;
        double _netAccum = 0;
        BifrostClient _bifrost = null;
        NetConfig _netConfig;

        double _physics_accum = 0;
        double _physics_InvtickRate = 1f / 60f;

        public AsgardClient() : base()
        {
            _netConfig = Config.Get<NetConfig>("network");

            _bifrost = new BifrostClient(_netConfig.Host, _netConfig.Port);
            AddInternalSystem(_bifrost, 0);

            float delay_amount = (float)Math.Round((1f / _netConfig.Tickrate) * 6f, 2);

            _jitterBuffer = new JitterBuffer<Tuple<Entity, NetworkObject>>(_netConfig.Tickrate);

        }

        public NetNode GetServerNode()
        {
            if (_bifrost.Peer == null ||
                _bifrost.Peer.Connections.Count == 0)
                return null;

            return (NetNode)_bifrost.Peer.Connections[0];
        }

        protected override void BeforeTick(double delta)
        {
            ObjectMapper.StartSnapshot();

            if (_pumpNetwork)
                _bifrost.pumpNetwork();

            var netObjects = ObjectMapper.EndSnapshot();
            if (netObjects.Count > 0)
            {
                _jitterBuffer.Add(netObjects);
            }
        }

        protected override void Tick(double delta)
        {
            _netAccum += delta;
            var invRate = 1f / (float)_netConfig.Tickrate;
            if (_netAccum >= invRate)
            {
                while (_netAccum >= invRate)
                {
                    _netAccum -= invRate;
                    SendClientState();
                    _bifrost.Flush();
                }
            }

            _physics_accum += delta;
            if (_physics_accum >= _physics_InvtickRate)
            {
                var ticks = 0;
                double time = NetTime.SimTime;
                while (_physics_accum >= _physics_InvtickRate)
                {
                    _physics_accum -= _physics_InvtickRate;

                    MergeJitterBuffer();
                    ticks++;
                }
            }
        }

        private void MergeJitterBuffer()
        {
            var netObjects = _jitterBuffer.Get();
            if (netObjects != null)
            {
                foreach(var netObj in netObjects)
                {
                    var entity = netObj.Item1;
                    var objB = netObj.Item2;
                    if (objB == null)
                    {
                        //TODO : LOG => should never happen
                        continue;
                    }

                    var objType = objB.GetType();
                    var compType = ComponentTypeManager.GetTypeFor(objType);
                    var objA =  entity.GetComponent(compType); 
                    if (objA == null)
                    {
                        entity.AddComponent(objB as IComponent);
                    }
                    else
                    {
                        var ditem = DataLookupTable.Get(objType.GetTypeInfo());
                        ditem.Merge(objA, objB);
                    }
                }
            }     
        }

        private void SendClientState()
        {
            if (GetServerNode() == null) return;

            var clientPacket = GetClientState();
            if (clientPacket == null) return;
            _bifrost.Send(clientPacket, 1);
        }

        protected virtual TClientData GetClientState()
        {
            return null;
        }
    }
}
