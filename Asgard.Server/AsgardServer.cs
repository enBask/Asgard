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
using Asgard.EntitySystems;
using FarseerPhysics.Common;
using Asgard.ScriptSystem;
using Asgard.Core.Network.RPC;

namespace Asgard
{

    public class AsgardServer : AsgardBase      
    {
        #region Private Vars
        BifrostServer _bifrost = null;

        protected NetConfig _netConfig;
        protected PhysicsConfig _phyConfig;
        protected ScriptConfig _scriptConfig;

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
            _scriptConfig = Config.Get<ScriptConfig>("script");


            PathHelpers.SetBasePath(_scriptConfig.ScriptPath);

            _bifrost = new BifrostServer(_netConfig.Port, _netConfig.MaxConnections);
            RPCManager.SetConnection(_bifrost);
            AddInternalSystem(_bifrost, 0);
            PacketFactory.AddCallback<AckStatePacket>(OnAckState);
            PacketFactory.AddCallback<ClientStatePacket>(OnClientState);

            PlayerSystem ps = new PlayerSystem();
            AddEntitySystem(ps);

        }

        private void OnClientState(ClientStatePacket clientState)
        {
            var playerSys = LookupSystem<PlayerSystem>();

            var conn = clientState.Connection;
            var player = playerSys.Get(conn);
            if (player == null) return;

            var playerComp = player.GetComponent<PlayerComponent>();

            foreach (var inp in clientState.State)
            {
                playerComp.InputBuffer.Add(inp);
            }
        }

        private void OnAckState(AckStatePacket obj)
        {
            var playerSys = LookupSystem<PlayerSystem>();
            if (playerSys != null)
            {
                var player = playerSys.Get(obj.Connection);
                if (player != null)
                {
                    var playerComp = player.GetComponent<PlayerComponent>();
                    if (playerComp != null)
                    {
                        playerComp.AckDeltaBaseline(obj.SimId);
                    }
                }
            }
        }

        protected virtual bool CanPlayerSee(Entity player, Entity checkPlayer)
        {
            return true;
        }

        protected virtual IEnumerable<Entity> GetPlayerList()
        {
            return EntityManager.GetEntities(Aspect.One(typeof(PlayerComponent)));
        }

        protected override void BeforeTick(double delta)
        {
            _bifrost.pumpNetwork();
        }

        protected override void AfterTick(double delta)
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

        protected override void BeforePhysics(float delta)
        {
            var ents = EntityManager.GetEntities(Aspect.All(typeof(PlayerComponent)));
            foreach(var e in ents)
            {
                var playerComp = e.GetComponent<PlayerComponent>();
                playerComp.GetNextState();
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
                        uint simTick = NetTime.SimTick;
                        var playerObj = entity.GetComponent<PlayerComponent>();
                        if (playerObj != null && playerObj.CurrentState != null)
                        {
                            simTick = playerObj.CurrentState.SimTick;
                        }
                        netSnycObj.Position = body.Position;
                        netSnycObj.LinearVelocity = body.LinearVelocity;
                        netSnycObj.Rotation = body.Rotation;
                        netSnycObj.SimTick = simTick;
                    }

                    var playerComp = entity.GetComponent<PlayerComponent>();
                    if (playerComp != null)
                    {
                        playerComp.RenderPosition = body.Position;
                        playerComp.RenderRotation = body.Rotation;
                    }

                }
            }
        }

        private void SendSnapshot()
        {
            var players = GetPlayerList();
            if (players == null) return;

            foreach (var player in players)
            {
                var playerComp = player.GetComponent<PlayerComponent>();

                List<DeltaLookup> deltaState = new List<DeltaLookup>();


                var node = playerComp.NetworkNode;
                if (node == null) continue;

                List<NetworkObject> fullProcList = new List<NetworkObject>();
                List<DeltaWrapper> _stateSendList = new List<DeltaWrapper>();
                List<DeltaWrapper> _defSendList = new List<DeltaWrapper>();
                List<DeltaLookup> deltaLookups = new List<DeltaLookup>();
                Dictionary<int, DeltaWrapper> packetBaseState = new Dictionary<int, DeltaWrapper>();

                foreach (Entity nobj in ObjectMapper.GetEntityCache())
                {
                    if (nobj != player && 
                        nobj.HasComponent<Physics2dComponent>() && 
                        !CanPlayerSee(player, nobj))
                    {
                        continue;
                    }

                    playerComp.trackEntity(nobj);

                    #region DefinitionSync
                    var defObjList = ObjectMapper.GetNetObjects(nobj, typeof(DefinitionNetworkObject));
                    if (defObjList != null)
                    {
                        foreach (DefinitionNetworkObject obj in defObjList)
                        {
                            fullProcList.Add(obj);
                            if (!playerComp.IsObjectKnown(obj))
                            {
                                playerComp.AddKnownObject(obj, (uint)nobj.UniqueId);
                                _defSendList.Add(new DeltaWrapper()
                                {
                                    Lookup = (uint)nobj.UniqueId,
                                    Object = obj
                                });
                            }
                        }
                    }
                    #endregion

                }

                #region StateSync
                var syncEntityList = playerComp.GetTrackedEntitiesByScore(10);
                foreach (var nobj in syncEntityList)
                {
                    var objList = ObjectMapper.GetNetObjects(nobj, typeof(StateSyncNetworkObject));
                    if (objList == null) continue;
                    foreach (var obj in objList)
                    {
                        var clone = obj.NetworkClone();
                        //quick hack to not send net sync data for the player controlled entity
                        //a different lag comp system is used.
                        if (clone is NetPhysicsObject)
                        {
                            //(clone as NetPhysicsObject).PlayerControlled = false;
                            var pComp = nobj.GetComponent<PlayerComponent>();
                            if (pComp != null)
                            {
                                if (pComp.NetworkNode == node)
                                {
                                    (clone as NetPhysicsObject).PlayerControlled = true;
                                }
                                else
                                {
                                    (clone as NetPhysicsObject).SimTick = 0; //don't send this to other clients
                                }
                            }
                            else
                            {
                                int ack = 0;
                            }
                        }

                        var baseline = playerComp.FindBaseline(obj);

                        deltaState.Add(new DeltaLookup()
                        {
                            Lookup = obj.GetHashCode(),
                            Object = clone
                        });

                        packetBaseState.Add(clone.GetHashCode(), baseline);

                        _stateSendList.Add(new DeltaWrapper()
                            {
                                Lookup = (uint)nobj.UniqueId,
                                Object = clone
                            });
                    }

                }
                #endregion

                #region send packets
                if (_stateSendList.Count > 0)
                {
                    var packet = new DataObjectPacket();
                    packet.Objects = _stateSendList;
                    packet.Method = NetDeliveryMethod.UnreliableSequenced;
                    packet.BaselineId = NetTime.SimTick;
                    packet.SetBaseline(packetBaseState);
                    _bifrost.Send(packet, node);
                }

                if (_defSendList.Count > 0)
                {
                    var packet = new DataObjectPacket();
                    packet.Method = NetDeliveryMethod.ReliableOrdered;
                    packet.Objects = _defSendList;
                    _bifrost.Send(packet, node, 3);
                }
                #endregion


                #region handle removed DefinitionNetworkObjects
                _defSendList.Clear();
                //cross check the fullproc list against the known def obj list of this player
                //this will tell us if the player is tracking an object that has been deleted
                var deletedItems = playerComp.FindDeletedObjects(fullProcList);
                foreach(var delObjItem in deletedItems)
                {
                    var delObj = delObjItem.Item1 as DefinitionNetworkObject;
                    var nobjId = delObjItem.Item2;
                    var marked = delObj.Destroy;

                    var ent = EntityManager.GetEntityByUniqueId(nobjId);
                    if (ent != null)
                    {
                        playerComp.UntrackEntity(ent);
                        var comps = EntityManager.GetComponents(ent);
                        foreach(var comp in comps)
                        {
                            var stateSyncComp = comp as StateSyncNetworkObject;
                            if (stateSyncComp != null)
                            {
                                playerComp.RemoveDeltaTrack(stateSyncComp);
                            }
                        }
                    }

                    delObj.Destroy = true;
                    playerComp.RemoveKnownObject(delObj);

                    _defSendList.Add(new DeltaWrapper()
                    {
                        Lookup = nobjId,
                        Object = delObj
                    });
                    if (!marked)
                        delObj.Destroy = false;
                }
                if (_defSendList.Count > 0)
                {
                    var packet = new DataObjectPacket();
                    packet.Method = NetDeliveryMethod.ReliableOrdered;
                    packet.Objects = _defSendList;
                    _bifrost.Send(packet, node, 3);
                }
                #endregion

                playerComp.AddDeltaState(deltaState);
            }                  
        }
    }
}
