using Asgard;
using Asgard.Core.Network.Packets;
using Asgard.EntitySystems;
using ChatServer;
using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Manager;
using Microsoft.Xna.Framework;
using Asgard.EntitySystems.Components;
using FarseerPhysics.Collision.Shapes;
using Artemis;
using Artemis.Utils;
using Asgard.Core.Network;
using Asgard.Core.System;
using Asgard.Core.Network.Data;

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
        PlayerSystem<PlayerData> _playerSys;
        int _worldId;

        public MoveServer()
        {
            _playerSys = new PlayerSystem<PlayerData>();
            AddEntitySystem(_playerSys);

            PacketFactory.AddCallback<MoveLoginPacket>(OnLogin);
            PacketFactory.AddCallback<ClientStatePacket>(OnClientState);
        }

        protected override void BeforeTick(double delta)
        {
            base.BeforeTick(delta);
        }

        double _accum = 0;
        double _InvtickRate = 1.0 / 60.0;
        protected override void Tick(double delta)
        {
            base.Tick(delta);

            _accum += delta;
            if (_accum >= _InvtickRate)
            {
                var ticks = 0;
                var time = NetTime.RealTime;
                while (_accum >= _InvtickRate)
                {
                    _accum -= _InvtickRate;

                    tickPhysics(_InvtickRate, time + (_InvtickRate * ticks));
                    ticks++;
                }
            }

        }

        private void tickPhysics(double delta, double time)
        {
            NetTime.SimTick++;
            var players = _playerSys.EntityManager.GetEntities(Aspect.One(typeof(PlayerData)));
            foreach (var player in players)
            {
                var phyComp = player.GetComponent<Physics2dComponent>();
                if (phyComp == null || phyComp.Body == null) continue;
                var playerData = player.GetComponent<PlayerData>();
                var StateData = playerData.GetNextState();
                if (StateData == null) continue;

                float x = 0f;
                float y = 0f;
                float speed = 100f;
                if (StateData.Forward)
                {
                    y = -speed;
                }
                if (StateData.Back)
                {
                    y = speed;
                }

                if (StateData.Right)
                {
                    x = speed;
                }
                if (StateData.Left)
                {
                    x = -speed;
                }

                var addX = (float)(x * delta);
                var addY = (float)(y * delta);
                phyComp.Body.Position += new Vector2(addX, addY);
                phyComp.Body.LinearVelocity = new Vector2(x, y);

                var dObject = player.GetComponent<DataObject>();
                if (dObject != null)
                {
                    dObject.X = phyComp.Body.Position.X;
                    dObject.Y = phyComp.Body.Position.Y;
                    dObject.VelX = x;
                    dObject.VelY = y;
                }

                // 
                //                  LogHelper.Log("Tick(" + NetTime.SimTick + ") =>" +
                //                      phyComp.Body.Position.X + "," + phyComp.Body.Position.Y 
                //                      +"," + addX+","+ addY + "-" + StateData.Y, "Server");
            }
        }

        private Dictionary<uint, int> _snapOffsets = new Dictionary<uint, int>();
        private void OnClientState(ClientStatePacket clientState)
        {
            var conn = clientState.Connection;
            var player = _playerSys.Get(conn);
            if (player == null) return;

            var phyComp = player.GetComponent<Physics2dComponent>();
            if (phyComp == null || phyComp.Body == null) return;


            phyComp.Body.UserData = clientState;
            var playerComp = player.GetComponent<PlayerData>();

            foreach (var inp in clientState.State)
            {
                var l = new List<PlayerStateData>();
                l.Add(inp);
                playerComp.InputBuffer.Add(l);
            }           

        }

        private void OnLogin(MoveLoginPacket packet)
        {
            var conn = packet.Connection;
            var playerEntity = _playerSys.Add(new PlayerData(networkNode: conn), 1);

            var phyData = new Physics2dComponent();
            phyData.WorldID = _worldId;
            phyData.StartingPosition = new Vector2(40f, 30f);
            phyData.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
            phyData.StartingRestitution = 1.0f;

            var shape = new CircleShape(1f, 0.001f);
            phyData.Shapes.Add(shape);

            playerEntity.AddComponent(phyData);

            var dObject = (DataObject)ObjectMapper.Create((uint)playerEntity.UniqueId, typeof(DataObject));

        }

        private void _bifrost_OnDisconnect(Asgard.Core.Network.NetNode connection)
        {
            var playerEntity = _playerSys.Get(connection);
            if (playerEntity != null)
            {
                _playerSys.Remove(playerEntity);
            }
        }


        Entity _box;
        public bool Start()
        {
            _bifrost = LookupSystem<BifrostServer>();
            _bifrost.OnDisconnect += _bifrost_OnDisconnect;

            var physicsSys = LookupSystem<PhysicsSystem2D>();
            _worldId = physicsSys.CreateWorld(new Vector2(0f, 0f));

            var world = physicsSys.GetWorld(_worldId);
            _box = physicsSys.EntityManager.Create();
            var physComp = new Physics2dComponent();
            physComp.BodyType = FarseerPhysics.Dynamics.BodyType.Static;
            physComp.StartingRestitution = 1.0f;

            EdgeShape es1 = new EdgeShape(new Vector2(0, 0), new Vector2(80f, 0f));
            EdgeShape es2 = new EdgeShape(new Vector2(80f, 0f), new Vector2(80f, 60f));
            EdgeShape es3 = new EdgeShape(new Vector2(80f, 60f), new Vector2(0, 60f));
            EdgeShape es4 = new EdgeShape(new Vector2(0, 60f), new Vector2(0f, 0f));

            physComp.Shapes.Add(es1);
            physComp.Shapes.Add(es2);
            physComp.Shapes.Add(es3);
            physComp.Shapes.Add(es4);

            _box.AddComponent(physComp);
            
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        protected override IEnumerable<int> GetPlayerList()
        {
            Bag<Entity> players =_playerSys.EntityManager.GetEntities(Aspect.One(typeof(PlayerData)));

            IEnumerable<int> playerIds = players.Select(e => e.Id);
            return playerIds;
        }

        protected override NetNode GetPlayerConnection(int entityId)
        {
            Entity player = _playerSys.EntityManager.GetEntity(entityId);
            var playerData = player.GetComponent<PlayerData>();
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
