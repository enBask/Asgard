using Asgard.Server.Network;
using Asgard.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis;
using Artemis.System;
using Asgard.Core.Network;
using Asgard.Core.System;
using Asgard.Core.Network.Packets;
using Asgard.Core.Interpolation;

namespace Asgard
{

    public class AsgardServer<TSnapshotPacket, TData> : AsgardBase
        where TData : class
        where TSnapshotPacket : Packet, IInterpolationPacket<TData>, new()
    {
        #region Private Vars
        BifrostServer _bifrost = null;

        protected NetConfig _netConfig;
        protected PhysicsConfig _phyConfig;

        float _netAccum = 0f;
        #endregion

        public AsgardServer() : base()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            _netConfig = Config.Get<NetConfig>("network");
            _phyConfig = Config.Get<PhysicsConfig>("physics");

            _bifrost = new BifrostServer(_netConfig.Port, _netConfig.MaxConnections);
            AddInternalSystem<BifrostServer>(_bifrost, 0);

            var physics = new PhysicsSystem2D( 1f / _phyConfig.tickrate );
            AddEntitySystem<PhysicsSystem2D>(physics);
        }

        protected virtual IEnumerable<int> GetPlayerList()
        {
            return null;
        }

        protected virtual List<TData> GetPlayerDataView(int entityId)
        {
            return null;
        }

        protected virtual NetNode GetPlayerConnection(int entityId)
        {
            return null;
        }


        protected override void Tick(float delta)
        {
            _netAccum += delta;
            var invRate = 1f / _netConfig.tickrate;
            if (_netAccum >= invRate)
            {
                var ticks = 0;
                while (_netAccum >= invRate)
                {
                    _netAccum -= invRate;
                    ticks++;
                }

                while (ticks-- > 0)
                {
                    SendSnapshot();
                }
            }
        }

        private void SendSnapshot()
        {
            uint snap_id = (uint)Math.Floor(_bifrost.NetTime * _netConfig.tickrate);

            var players = GetPlayerList();
            if (players == null) return;

            foreach(var player in players)
            {
                var node = GetPlayerConnection(player);
                if (node == null) continue;

                var data = GetPlayerDataView(player);
                var snapPacket = new TSnapshotPacket();
                snapPacket.Id = snap_id;
                snapPacket.DataPoints = data;
                _bifrost.Send(snapPacket, node);
            }            
        }

    }
}
