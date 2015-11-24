using Asgard.Client.Collections;
using Asgard.Client.Network;
using Asgard.Core.Interpolation;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using Asgard.Core.System;
using Asgard.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Client
{
    public class AsgardClient<TSnapshotPacket, TData> :  AsgardBase
        where TData : class
        where TSnapshotPacket: Packet, IInterpolationPacket<TData>
    {

        public delegate void AsgardClientCallback();
        public delegate void SnapshotCallback(TSnapshotPacket snapshot);

        public event SnapshotCallback OnSnapshot;

        InterpolationBuffer<TSnapshotPacket, TData> _interpolationBuffer;
        bool started = false;
        float startTime = 0f;

        BifrostClient _bifrost = null;
        NetConfig _netConfig;
        public AsgardClient() : base()
        {
            _netConfig = Config.Get<NetConfig>("network");

            _bifrost = new BifrostClient(_netConfig.Host, _netConfig.Port);
            AddInternalSystem(_bifrost, 0);

            float delay_amount = (float)Math.Round((1f / _netConfig.Tickrate) * 6f, 2);
            _interpolationBuffer =
                new InterpolationBuffer<TSnapshotPacket, TData>(_netConfig.Tickrate, delay_amount);

            PacketFactory.AddCallback<TSnapshotPacket>(_OnSnapshot);

        }

        private void _OnSnapshot(TSnapshotPacket snapPacket)
        {
            _interpolationBuffer.Add(snapPacket);
            if (!started)
            {
                started = true;
                startTime = (float)snapPacket.ReceiveTime;
            }

            if (OnSnapshot != null)
            {
                OnSnapshot(snapPacket);
            }
        }

        private NetNode GetServerNode()
        {
            if (_bifrost.Peer == null ||
                _bifrost.Peer.Connections.Count == 0)
                return null;

            return (NetNode)_bifrost.Peer.Connections[0];
        }

        protected override void Tick(float delta)
        {
        }

        public List<TData> GetInterpolationObjects()
        {
            var serverNode = GetServerNode();
            if (serverNode == null) return null;
            var netTime = _bifrost.NetTime;
            var remoteTime = serverNode.GetRemoteTime(netTime);
            return _interpolationBuffer.Update((float)remoteTime);
        }
    }
}
