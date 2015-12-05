using Artemis;
using Asgard;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using Asgard.Core.Physics;
using Asgard.Core.System;
using Asgard.EntitySystems.Components;
using ChatServer;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Farseer.Framework;
using MoveClient;
using MoveServer;
using System;
using System.Collections.Generic;

namespace ChatClient
{
    public static class LogHelper
    {
        private static string _lastLog;
        public static void Log(string data, string cat = "")
        {
            if (data == _lastLog)
                return;

            _lastLog = data;

            System.Diagnostics.Trace.WriteLine(data, cat);
        }
    }

    public class MoveClient : Asgard.Client.AsgardClient<ClientStatePacket>
    {

        public delegate void OnTickCallback(double delta);
        public event OnTickCallback OnTick;

        BifrostClient _bifrost;

        public PlayerStateData PlayerState { get; set; }
        public List<PlayerStateData> StateList { get; set; }

        Asgard.Core.Collections.LinkedList<PlayerStateData> _movebuffer = 
            new Asgard.Core.Collections.LinkedList<PlayerStateData>();


        public MoveClient() : base()
        {
            _bifrost = LookupSystem<BifrostClient>();
            StateList = new List<PlayerStateData>();

            _bifrost.OnDisconnect += ChatClient_OnDisconnect;
            _bifrost.OnConnection += ChatClient_OnConnection;
            PacketFactory.AddCallback<LoginResponsePacket>(OnLoginResult);

            var midgard = LookupSystem<Midgard>();
            var def = new BodyDefinition();
            var body = midgard.CreateBody(def);
            body.BodyType = FarseerPhysics.Dynamics.BodyType.Static;
            Vertices verts = new Vertices(4);
            verts.Add(new Vector2(0, 0));
            verts.Add(new Vector2(0, -60));
            verts.Add(new Vector2(80, -60));
            verts.Add(new Vector2(80, 0));


            EdgeShape shape = new EdgeShape(new Vector2(0, 0), new Vector2(80, 0));
            var fix = body.CreateFixture(shape);
            fix.Restitution = 1.0f;
            shape = new EdgeShape(new Vector2(80, 0), new Vector2(80, 60));
            fix = body.CreateFixture(shape);
            fix.Restitution = 1.0f;
            shape = new EdgeShape(new Vector2(80, 60), new Vector2(0, 60));
            fix = body.CreateFixture(shape);
            fix.Restitution = 1.0f;
            shape = new EdgeShape(new Vector2(0, 60), new Vector2(0, 0));
            fix = body.CreateFixture(shape);
            fix.Restitution = 1.0f;

        }

        public void PumpNetwork(bool pump)
        {
            _pumpNetwork = pump;
        }

        private void ChatClient_OnConnection(NetNode connection)
        {
            Console.WriteLine("Connected");
            Login();
        }

        private void ChatClient_OnDisconnect(NetNode connection)
        {
            Connect();
        }

        private void OnLoginResult(LoginResponsePacket packet)
        {
            var midgard = LookupSystem<Midgard>();

            var playerData = new PlayerComponent(networkNode: packet.Connection);
            var playerEntity = EntityManager.Create(1);
            playerEntity.AddComponent(playerData);
        }

        private void Connect()
        {
            _bifrost.Start();
        }

        private void Login()
        {
            MoveLoginPacket loginPacket = new MoveLoginPacket();
            _bifrost.Send(loginPacket);
        }

        protected override ClientStatePacket GetClientState()
        {
            ClientStatePacket packet = new ClientStatePacket();
            packet.State = new List<Asgard.Core.System.PlayerStateData>(StateList);
            StateList.Clear();
            packet.SnapId = (int)NetTime.SimTick;

            return packet;
        }

        protected override void Tick(double delta)
        {
            base.Tick(delta);
            if (OnTick != null)
                OnTick(delta);

            var player = EntityManager.GetEntityByUniqueId(1);
            if (player == null) return;
            var playerComp = player.GetComponent<PlayerComponent>();
            var phyComp = player.GetComponent<Physics2dComponent>();
            if (phyComp == null || phyComp.Body == null) return;

            if (playerComp.LerpToReal)
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
                playerComp.OldPosition = phyComp.Body.Position;
                playerComp.RenderPosition = phyComp.Body.Position;
            }


        }

        protected override void BeforePhysics(float delta)
        {
            base.BeforePhysics(delta);

            Vector2 vel = new Vector2();
            float speed = 25f;
            if (PlayerState.Forward)
            {
                vel.Y = -speed;
            }
            if (PlayerState.Back)
            {
                vel.Y = speed;
            }

            if (PlayerState.Right)
            {
                vel.X = speed;
            }
            if (PlayerState.Left)
            {
                vel.X = -speed;
            }

            var player = EntityManager.GetEntityByUniqueId(1);
            if (player == null) return;
            var pCompo = player.GetComponent<Physics2dComponent>();
            if (pCompo == null) return;
            pCompo.Body.LinearVelocity = vel;


            ApplyLagComp();
           

        }

        private void ApplyLagComp()
        {
            var player = EntityManager.GetEntityByUniqueId(1);
            var netSync = player.GetComponent<NetPhysicsObject>();
            if (netSync == null) return;

            Asgard.Core.Collections.LinkedListNode<PlayerStateData> found_node = null;
            foreach(var node in _movebuffer)
            {
                if (node.Value.SimTick == netSync.SimTick)
                {
                    found_node = node;
                    break;
                }
            }

            if (found_node != null)
            {
                var moveData = found_node.Value;

                var diff = netSync.Position -moveData.Position;

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

                    var pComp = player.GetComponent<Physics2dComponent>();
                    if (pComp == null || pComp.Body == null) return;

                    var playerComponent = player.GetComponent<PlayerComponent>();
                    playerComponent.LerpToReal = true;
                    playerComponent.OldPosition = pComp.Body.Position;
                    playerComponent.LerpStart = NetTime.RealTime;
                    playerComponent.LerpEnd = playerComponent.LerpStart + (0.1f);
                    pComp.Body.Position = pos;
                }


                _movebuffer.TruncateTo(found_node.Next);
            }


        }
        protected override void AfterPhysics(float delta)
        {
            base.AfterPhysics(delta);

            var player = EntityManager.GetEntityByUniqueId(1);
            if (player == null) return;
            var pCompo = player.GetComponent<Physics2dComponent>();
            if (pCompo == null) return;

            var move = new PlayerStateData()
            {
                Position = pCompo.Body.Position,
                Left = PlayerState.Left,
                Right = PlayerState.Right,
                Forward = PlayerState.Forward,
                Back = PlayerState.Back,
                SimTick = NetTime.SimTick
            };

            StateList.Add(move);
            _movebuffer.AddToTail(move);
        }
    }
}
