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
            AddInternalSystem(_bifrost, 0);

            PacketFactory.AddCallback<AckStatePacket>(OnAckState);
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

                        uint simTick = NetTime.SimTick;
                        var playerObj = entity.GetComponent<PlayerComponent>();
                        if (playerObj != null && playerObj.CurrentState != null)
                        {
                            simTick = playerObj.CurrentState.SimTick;
                        }
                        netSnycObj.Position = body.Position;
                        netSnycObj.LinearVelocity = body.LinearVelocity;
                        netSnycObj.SimTick = simTick;
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

                var baselineState = playerComp.GetDeltaBaseline();
                var usebaseLine = (baselineState != null && baselineState.Item1 != 0);
                Dictionary<int, NetworkObject> deltaState =
                   new Dictionary<int, NetworkObject>();


                var node = playerComp.NetworkNode;
                if (node == null) continue;

                List<NetworkObject> fullProcList = new List<NetworkObject>();
                List<Tuple<uint, NetworkObject>> _stateSendList = new List<Tuple<uint, NetworkObject>>();
                List<Tuple<uint, NetworkObject>> _defSendList = new List<Tuple<uint, NetworkObject>>();
                Dictionary<int, NetworkObject> packetBaseState = new Dictionary<int, NetworkObject>();

                foreach (Entity nobj in ObjectMapper.GetEntityCache())
                {
                    if (nobj != player && 
                        nobj.HasComponent<Physics2dComponent>() && 
                        !CanPlayerSee(player, nobj))
                    {
                        continue;
                    }

                    #region StateSync
                    var objList = ObjectMapper.GetNetObjects(nobj, typeof(StateSyncNetworkObject));
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

                        NetworkObject baseClone = null;
                        if (baselineState != null)
                        {
                            baselineState.Item2.TryGetValue(obj.GetHashCode(), out baseClone);
                        }
                        deltaState.Add(obj.GetHashCode(), clone);
                        packetBaseState.Add(clone.GetHashCode(), baseClone);

                        _stateSendList.Add(new Tuple<uint, NetworkObject>((uint)nobj.UniqueId, clone));
                    }

                  
                    #endregion

                    #region DefinitionSync
                    var defObjList = ObjectMapper.GetNetObjects(nobj, typeof(DefinitionNetworkObject));
                    foreach (DefinitionNetworkObject obj in defObjList)
                    {
                        fullProcList.Add(obj);
                        if (!playerComp.IsObjectKnown(obj))
                        {
                            playerComp.AddKnownObject(obj, (uint)nobj.UniqueId);
                            _defSendList.Add(new Tuple<uint, NetworkObject>((uint)nobj.UniqueId, obj));
                        }
                    }
                    #endregion
                }

                if (_stateSendList.Count > 0)
                {
                    var packet = new DataObjectPacket();
                    packet.Objects = _stateSendList;
                    packet.Method = NetDeliveryMethod.UnreliableSequenced;
                    if (usebaseLine)
                    {
                        packet.BaselineId = baselineState.Item1;
                        packet.SetBaseline(packetBaseState);
                    }
                    _bifrost.Send(packet, node);
                }

                if (_defSendList.Count > 0)
                {
                    var packet = new DataObjectPacket();
                    packet.Method = NetDeliveryMethod.ReliableOrdered;
                    packet.Objects = _defSendList;
                    _bifrost.Send(packet, node, 3);
                }



                #region handle removed DefinitionNetworkObjects
                _defSendList.Clear();
                //cross check the fullproc list against the known def obj list of this player
                //this will tell us if the player is tracking an object that has been deleted
                var deletedItems = playerComp.FindDeletedObjects(fullProcList);
                foreach(var delObjItem in deletedItems)
                {
                    var delObj = delObjItem.Item1 as DefinitionNetworkObject;
                    var nobjId = delObjItem.Item2;
                    var marked = delObj.Destory;
                    delObj.Destory = true;
                    playerComp.RemoveKnownObject(delObj);

                    _defSendList.Add(new Tuple<uint, NetworkObject>(nobjId, delObj));
                    if (!marked)
                        delObj.Destory = false;
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
