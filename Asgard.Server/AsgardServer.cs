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
using Asgard.Core.Network.Data;
using Asgard.Core.Physics;

namespace Asgard
{

    public class AsgardServer : AsgardBase      
    {
        #region Private Vars
        BifrostServer _bifrost = null;

        protected NetConfig _netConfig;
        protected PhysicsConfig _phyConfig;

        double _netAccum = 0;
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
        }

        protected virtual IEnumerable<int> GetPlayerList()
        {
            return null;
        }
       
        protected virtual NetNode GetPlayerConnection(int entityId)
        {
            return null;
        }

        protected override void BeforeTick(double delta)
        {
            _bifrost.pumpNetwork();
        }

        protected override void Tick(double delta)
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
            var players = GetPlayerList();
            if (players == null) return;

            foreach(Entity nobj in ObjectMapper.GetEntityCache())
            {
                var objList = ObjectMapper.GetNetObjects(nobj);
                foreach(var obj in objList)
                {
                    var packet = new DataObjectPacket();
                    packet.SetOwnerObject(obj);
                    packet.Id = (uint)nobj.UniqueId;

                    foreach (var player in players)
                    {
                        var node = GetPlayerConnection(player);
                        if (node == null) continue;
                        _bifrost.Send(packet, node);
                    }
                }
            }                    
        }
    }
}
