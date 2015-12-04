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
using Microsoft.Xna.Framework;
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

           

        }

        protected override void AfterPhysics(float delta)
        {
            base.AfterPhysics(delta);

            var player = EntityManager.GetEntityByUniqueId(1);
            if (player == null) return;
            var pCompo = player.GetComponent<Physics2dComponent>();
            if (pCompo == null) return;

            StateList.Add(new PlayerStateData()
            {
                Position = pCompo.Body.Position,
                Left = PlayerState.Left,
                Right = PlayerState.Right,
                Forward = PlayerState.Forward,
                Back = PlayerState.Back
            });


            
        }
    }
}
