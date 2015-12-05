using Asgard;
using Asgard.Core.Network.Packets;
using Asgard.Core.System;
using Asgard.EntitySystems;
using ChatServer;
using System.Collections.Generic;
using System.Linq;
using Asgard.EntitySystems.Components;
using Artemis;
using Artemis.Utils;
using Asgard.Core.Network;
using Asgard.Core.Physics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Collision.Shapes;

namespace MoveServer
{

    public static class LogHelper
    {
        private static string _lastLog;
        public static void Log(string data, string cat="")
        {
            if (data == _lastLog)
                return;

            _lastLog = data;

            System.Diagnostics.Trace.WriteLine(data, cat);
        }
    }

    public class MoveServer : AsgardServer
    {
        BifrostServer _bifrost;
        PlayerSystem _playerSys;

        List<Ball> _balls = new List<Ball>();

        public MoveServer()
        {
            _playerSys = new PlayerSystem();
            AddEntitySystem(_playerSys);

            PacketFactory.AddCallback<MoveLoginPacket>(OnLogin);
            PacketFactory.AddCallback<ClientStatePacket>(OnClientState);

            var midgard = LookupSystem<Midgard>();

            var def = new BodyDefinition();
            var body = midgard.CreateBody(def);
            body.BodyType = FarseerPhysics.Dynamics.BodyType.Static;

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

        protected override void BeforePhysics(float delta)
        {
            base.BeforePhysics(delta);

            var players = EntityManager.GetEntities(Aspect.One(typeof(PlayerComponent)));
            foreach (var player in players)
            {
                var pComponent = player.GetComponent<PlayerComponent>();
                var phyComp = player.GetComponent<Physics2dComponent>();
                if (phyComp == null || phyComp.Body == null)
                {
                    continue;
                }

                var stateData = pComponent.GetNextState();
                if (stateData == null) continue;

                float speed = 25f;
                Vector2 vel = new Vector2();
                if (stateData.Forward)
                {
                    vel.Y = -speed;
                }
                if (stateData.Back)
                {
                    vel.Y = speed;
                }

                if (stateData.Right)
                {
                    vel.X = speed;
                }
                if (stateData.Left)
                {
                    vel.X = -speed;
                }

                phyComp.Body.LinearVelocity = vel;


            }
        }

        private void OnClientState(ClientStatePacket clientState)
        {
            var conn = clientState.Connection;
            var player = _playerSys.Get(conn);
            if (player == null) return;

            var playerComp = player.GetComponent<PlayerComponent>();

            foreach (var inp in clientState.State)
            {
                var l = new List<PlayerStateData>();
                l.Add(inp);
                playerComp.InputBuffer.Add(l);
            }           
        }

        private void OnLogin(MoveLoginPacket packet)
        {
            var midgard = LookupSystem<Midgard>();

            var conn = packet.Connection;
            var playerData = new PlayerComponent(networkNode: conn);
            var playerEntity = _playerSys.Add(playerData, 1);

            var ball = (Ball)ObjectMapper.Create((uint)playerEntity.UniqueId, typeof(Ball));
            ball.BodyDef = new BodyDefinition() { Position = new Vector2(40f, 30f) };
            ball.Setup(midgard, 1);
            _balls.Add(ball);

            {

                var pe = midgard.EntityManager.Create(2);
                var remoteBall = (Ball)ObjectMapper.Create((uint)pe.UniqueId, typeof(Ball));
                remoteBall.BodyDef = 
                    new BodyDefinition()
                    {
                        Position = new Vector2(0f, 0f),
                        LinearVelocity = new Vector2(2f, 2f)
                    };
                remoteBall.Setup(midgard, 2);
                _balls.Add(remoteBall);
            }

            var response = new LoginResponsePacket();
            _bifrost.Send(response, packet.Connection);
        }

        private void _bifrost_OnDisconnect(Asgard.Core.Network.NetNode connection)
        {
            var playerEntity = _playerSys.Get(connection);
            if (playerEntity != null)
            {
                _playerSys.Remove(playerEntity);
            }
        }

        public bool Start()
        {
            _bifrost = LookupSystem<BifrostServer>();
            _bifrost.OnDisconnect += _bifrost_OnDisconnect;            
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        protected override IEnumerable<int> GetPlayerList()
        {
            Bag<Entity> players =_playerSys.EntityManager.GetEntities(Aspect.One(typeof(PlayerComponent)));

            IEnumerable<int> playerIds = players.Select(e => e.Id);
            return playerIds;
        }

        protected override NetNode GetPlayerConnection(int entityId)
        {
            Entity player = _playerSys.EntityManager.GetEntity(entityId);
            var playerData = player.GetComponent<PlayerComponent>();
            var node = playerData.NetworkNode;
            return node;
        }

//         protected override List<MoveData> GetPlayerDataView(int entityId)
//         {
//             List<MoveData> moveDataSet = new List<MoveData>();
// 
//             Bag<Entity> players = _playerSys.EntityManager.GetEntities(Aspect.One(typeof(PlayerData)));
// 
//             var thisPlayer = _playerSys.EntityManager.GetEntity(entityId);
//             var phyData = thisPlayer.GetComponent<Physics2dComponent>();
//             if (phyData != null && phyData.Body != null)
//             {
//                 int snap_id = 0;
//                 if (!_snapOffsets.TryGetValue(NetTime.SimTick, out snap_id))
//                     snap_id = (int)NetTime.SimTick;
// 
//                 var moveData = new MoveData(phyData.Body.Position.X, phyData.Body.Position.Y);
//                 moveData.VelX = phyData.Body.LinearVelocity.X;
//                 moveData.VelY = phyData.Body.LinearVelocity.Y;
//                 moveData.Id = entityId;
//                 moveData.SnapId = snap_id;
//                 moveDataSet.Add(moveData);
// 
// 
// 
// //                 LogHelper.Log("ClientSend(" + sim_id + ") =>" +
// //                     snap_id, "Server");
//             }
// 
//             foreach (var player in players)
//             {
//                 if (thisPlayer == player) continue;
//                 phyData = player.GetComponent<Physics2dComponent>();
//                 if (phyData == null || phyData.Body == null) continue;
// 
//                 var moveData = new MoveData(phyData.Body.Position.X, phyData.Body.Position.Y);
//                 moveData.VelX = phyData.Body.LinearVelocity.X;
//                 moveData.VelY = phyData.Body.LinearVelocity.Y;
//                 moveData.Id = player.Id;
// 
//                 moveDataSet.Add(moveData);
//             }
// 
//             return moveDataSet;
//         }

    }
}
