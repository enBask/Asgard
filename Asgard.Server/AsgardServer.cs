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
using Asgard.EntitySystems.Components;

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

        protected override void AfterPhysics(float delta)
        {
            var world = LookupSystem<Midgard>();
            foreach (var body in world.BodyList)
            {
                var entity = body.UserData as Entity;
                if (entity != null)
                {
                    var netSnycObj = entity.GetComponent<NetPhysicsObject>();
                    if (netSnycObj != null)
                    {
                        netSnycObj.Position = body.Position;
                        netSnycObj.LinearVelocity = body.LinearVelocity;
                    }
                }
            }
        }

        private void SendSnapshot()
        {
            var players = GetPlayerList();
            if (players == null) return;

            foreach(Entity nobj in ObjectMapper.GetEntityCache())
            {
                var objList = ObjectMapper.GetNetObjects(nobj, typeof(StateSyncNetworkObject));
                foreach(var obj in objList)
                {
                    foreach (var player in players)
                    {
                        var node = GetPlayerConnection(player);
                        if (node == null) continue;

                        //quick hack to not send net sync data for the player controlled entity
                        //a different lag comp system is used.
                        if (obj is NetPhysicsObject)
                        {
                            var pComp = nobj.GetComponent<PlayerComponent>();
                            if (pComp != null)
                            {
                                if (pComp.NetworkNode == node)
                                    continue;
                            }
                        }


                        var packet = new DataObjectPacket();
                        packet.SetOwnerObject(obj);
                        packet.Id = (uint)nobj.UniqueId;

                        _bifrost.Send(packet, node);
                    }
                }

                var defObjList = ObjectMapper.GetNetObjects(nobj, typeof(DefinitionNetworkObject));
                foreach (var obj in defObjList)
                {
                    foreach (var player in players)
                    {
                        var node = GetPlayerConnection(player);
                        if (node == null) continue;

                        var entity = EntityManager.GetEntity(player);
                        var playerComp = entity.GetComponent<PlayerComponent>();
                        if (!playerComp.IsObjectKnown(obj))
                        {
                            playerComp.AddKnownObject(obj);
                            var packet = new DataObjectPacket();
                            packet.SetOwnerObject(obj);
                            packet.Id = (uint)nobj.UniqueId;

                            _bifrost.Send(packet, node, 3);
                        }
                    }
                }       
            }                    
        }
    }
}
