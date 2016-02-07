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
using Asgard.Core.Network.RPC;
using Asgard.Core.Physics;
using Asgard.Core.System;
using Asgard.Core.Utils;
using Asgard.EntitySystems.Components;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Client
{
    public abstract class AsgardClient : AsgardBase
    {

        protected bool _pumpNetwork = true;

        public delegate void AsgardClientCallback();

        List<PlayerStateData> _stateList;
        Core.Collections.LinkedList<PlayerStateData> _moveBuffer;
        JitterBuffer<List<Tuple<Entity,NetworkObject>>> _jitterBuffer;

        double _netAccum = 0;
        BifrostClient _bifrost = null;
        NetConfig _netConfig;

        public AsgardClient() : base()
        {
            _netConfig = Config.Get<NetConfig>("network");

            _bifrost = new BifrostClient(_netConfig.Host, _netConfig.Port);
            RPCManager.SetConnection(_bifrost);
            AddInternalSystem(_bifrost, 0);

            float delay_amount = (float)Math.Round((1f / _netConfig.Tickrate) * 6f, 2);

            _jitterBuffer = new JitterBuffer<List<Tuple<Entity, NetworkObject>>>(_netConfig.Tickrate);
            _stateList = new List<PlayerStateData>();
            _moveBuffer = new Core.Collections.LinkedList<PlayerStateData>();

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

                var simid = ObjectMapper.GetLastSimId();
                if (simid != 0)
                {
                    AckStatePacket p = new AckStatePacket();
                    p.SimId = simid;
                    _bifrost.Send(p, 3);
                }
            }
        }

        protected override void AfterTick(double delta)
        {
            _netAccum += delta;
            var invRate = 1f / (float)_netConfig.Tickrate;
            if (_netAccum >= invRate)
            {
                while (_netAccum >= invRate)
                {
                    _netAccum -= invRate;
                    SendClientState();
                }
            }

            #region client lag comp smoothing
            //smooth client movement against lag comp.
            var thisPlayer = GetPlayer();
            if (thisPlayer == null) return;
            var playerComp = thisPlayer.GetComponent<PlayerComponent>();
            var phyComp = thisPlayer.GetComponent<Physics2dComponent>();
            if (phyComp == null || phyComp.Body == null) return;


            var d = (phyComp.Body.Position - playerComp.OldPosition).LengthSquared();
            if (playerComp.LerpToReal && d < 5f)
            {
                float t = (float)((NetTime.RealTime - playerComp.LerpStart)
                    / (playerComp.LerpEnd - playerComp.LerpStart));
                t = Math.Min(t, 1.0f);
                t = Math.Max(t, 0.0f);

                playerComp.RenderPosition = Vector2.Lerp(playerComp.OldPosition, phyComp.Body.Position, t);
                if ((playerComp.OldPosition - phyComp.Body.Position).LengthSquared() <= 0.0001f)
                {
                    playerComp.LerpToReal = false;
                }
            }
            else
            {
                playerComp.LerpToReal = false;
                playerComp.OldPosition = phyComp.Body.Position;
                playerComp.RenderPosition = phyComp.Body.Position;
                playerComp.RenderRotation = phyComp.Body.Rotation;
            }
            #endregion

        }

        protected override void BeforePhysics(float delta)
        {
            base.BeforePhysics(delta);
            MergeJitterBuffer();
            ApplyLagComp();

            var ents = EntityManager.GetEntities(Aspect.One(typeof(NetPhysicsObject)));
            foreach (var ent in ents)
            {
                var dObj = ent.GetComponent<NetPhysicsObject>();
                float X = dObj.position_error.X;
                float Y = dObj.position_error.Y;

                if (Math.Abs(dObj.position_error.X) >= 0.00001f)
                    if (Math.Abs(dObj.position_error.X) >= 1f)
                        X *= 0.975f;
                    else
                        X *= 0.975f;
                else
                    X = 0;

                if (Math.Abs(dObj.position_error.Y) >= 0.00001f)
                    if (Math.Abs(dObj.position_error.Y) >= 1f)
                        Y *= 0.975f;
                    else
                        Y *= 0.975f;
                else
                    Y = 0;

                var phyComp = ent.GetComponent<Physics2dComponent>();
                if (phyComp != null && phyComp.Body != null)
                {
                    float d = dObj.rotation_slerp - phyComp.Body.Rotation;


                    if (!dObj.PlayerControlled && Math.Abs(d) >= 0.01f)
                    {
                        dObj.rotation_slerp = phyComp.Body.Rotation;
                        //                    dObj.rotation_slerp = MathHelper.Lerp(dObj.rotation_slerp, phyComp.Body.Rotation, 0.2f);
                        //                     System.Diagnostics.Trace.WriteLine("slerp => " + dObj.rotation_slerp + " " 
                        //                         + dObj.rotation_error + " " + phyComp.Body.Rotation);
                        //dObj.rotation_error = dObj.rotation_slerp;
                    }
                    else if (!dObj.PlayerControlled)
                    {
                        //                    System.Diagnostics.Trace.WriteLine("break out => " + phyComp.Body.Rotation);
                        dObj.rotation_slerp = phyComp.Body.Rotation;
                    }
                }


                dObj.position_error = new Vector2(X, Y);
            }

        }
        protected override void AfterPhysics(float delta)
        {
            base.AfterPhysics(delta);

            var ents = EntityManager.GetEntities(Aspect.One(typeof(NetPhysicsObject)));
            foreach (var ent in ents)
            {
                var pComp = ent.GetComponent<Physics2dComponent>();
                var dObj = ent.GetComponent<NetPhysicsObject>();
                if (pComp == null || dObj == null || pComp.Body == null)
                    continue;

                if (dObj.IsUpdated)
                {
                    if (!dObj.PlayerControlled)
                    {
//                        Trace.WriteLine("new rot => " + dObj.Rotation);
                        dObj.position_error = (pComp.Body.Position + dObj.position_error) - dObj.Position;
                        dObj.rotation_error = pComp.Body.Rotation;
                        if (dObj.position_error.LengthSquared() > 5f)
                        {
                            dObj.position_error = Farseer.Framework.Vector2.Zero;
                        }

                        pComp.Body.Position = dObj.Position;
                        pComp.Body.LinearVelocity = dObj.LinearVelocity;
                        pComp.Body.Rotation = dObj.Rotation;
                    }

                    dObj.IsUpdated = false;
                }
            }
           
        }

        private void ApplyLagComp()
        {
            var thisPlayer = GetPlayer();
            if (thisPlayer == null) return;

            var netSync = thisPlayer.GetComponent<NetPhysicsObject>();
            if (netSync == null) return;

            Asgard.Core.Collections.LinkedListNode<PlayerStateData> found_node = null;
            foreach (var node in _moveBuffer)
            {
                if (node.Value.SimTick.Value == netSync.SimTick.Value)
                {
                    found_node = node;
                    break;
                }
            }

            if (found_node != null)
            {
                var moveData = found_node.Value;

                var diff = netSync.Position.Value - moveData.Position.Value;

                if (diff.LengthSquared() > 0)
                {
                    var node = found_node;
                    Vector2 pos = netSync.Position;

                    Vector2 prev_pos = node.Value.Position;
                    while (node != null)
                    {
                        var move = node.Value;
                        Vector2 velStep = move.Position - prev_pos;


                        pos += velStep;
                        prev_pos = move.Position;
                        node = node.Next;
                        move.Position = pos;
                    }

                    var pComp = thisPlayer.GetComponent<Physics2dComponent>();
                    if (pComp == null || pComp.Body == null) return;

                    var playerComponent = thisPlayer.GetComponent<PlayerComponent>();
                    playerComponent.LerpToReal = true;
                    playerComponent.OldPosition = pComp.Body.Position;
                    playerComponent.LerpStart = NetTime.RealTime;
                    playerComponent.LerpEnd = playerComponent.LerpStart + (0.1f);
                    pComp.Body.Position = pos;
                }


                _moveBuffer.TruncateTo(found_node.Next);
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

                    if (netObj.Item2 is DefinitionNetworkObject)
                    {
                        if ((netObj.Item2 as DefinitionNetworkObject).Destroy)
                        {
                            var defObA = entity.GetComponent(compType);
                            if (defObA != null)
                            {
                                var ditem = DataLookupTable.Get(objType.GetTypeInfo());
                                foreach (var prop in ditem.Properties)
                                {
                                    if (prop.ChildProperty != null)
                                    {
                                        prop.UnDefineObject(defObA as NetworkObject, entity);
                                    }
                                }
                                ObjectMapper.UnDefineObject(defObA, entity.UniqueId);
                            }
                            continue;
                        }
                        else
                        {
                            var ditem = DataLookupTable.Get(objType.GetTypeInfo());
                            foreach (var prop in ditem.Properties)
                            {
                                if (prop.ChildProperty != null)
                                {
                                    prop.DefineObject(objB, entity);
                                }
                            }
                            ObjectMapper.DefineObject(objB, entity.UniqueId);

                        }
                    }

                    var objA =  entity.GetComponent(compType); 
                    if (objA == null)
                    {
                        objB.Owner = entity;
                        entity.AddComponent(objB as IComponent);
                    }
                    else
                    {
                        var ditem = DataLookupTable.Get(objType.GetTypeInfo());

                        ditem.Merge((NetworkObject)objA, objB);
                    }
                }
            }     
        }

        protected void AddClientState(PlayerStateData state)
        {
            _stateList.Add(state);
            _moveBuffer.AddToTail(state);
        }
        private void SendClientState()
        {
            if (GetServerNode() == null) return;

            var states = GetClientState();
            if (states == null) return;

            var playerEnt = GetPlayer();
            if (playerEnt == null) return;

            ClientStatePacket packet = new ClientStatePacket();
            packet.State = states;
            packet.PreviousState = _lastClientState;
            packet.PlayerId = (uint)playerEnt.UniqueId;
            _lastClientState = states.Count > 0 ? states.Last() : null;
            _bifrost.Send(packet, 1);
        }

        private PlayerStateData _lastClientState = null;
        private List<PlayerStateData> GetClientState()
        {
            var states = new List<PlayerStateData>(_stateList);
            _stateList.Clear();
            return states;
        }

        protected abstract Entity GetPlayer();
    }
}
