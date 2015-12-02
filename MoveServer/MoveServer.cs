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
using Asgard.Core.System;
using System.Numerics;
using Asgard.Core.Physics;

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
        int _worldId;

        public MoveServer()
        {
            _playerSys = new PlayerSystem();
            AddEntitySystem(_playerSys);

            PacketFactory.AddCallback<MoveLoginPacket>(OnLogin);
            PacketFactory.AddCallback<ClientStatePacket>(OnClientState);
        }

        protected override void BeforeTick(double delta)
        {
            base.BeforeTick(delta);
        }

        protected override void Tick(double delta)
        {
            base.Tick(delta);

            var players = _playerSys.EntityManager.GetEntities(Aspect.One(typeof(PlayerComponent)));
            foreach(var player in players)
            {
                var pComp = player.GetComponent<PlayerComponent>();
                var dObject = player.GetComponent<DataObject>();
                if (dObject != null)
                {
                    dObject.X = pComp.Body.Position.X;
                    dObject.Y = pComp.Body.Position.Y;
                    dObject.VelX = pComp.Body.LinearVelocity.X;
                    dObject.VelY = pComp.Body.LinearVelocity.Y;
                }
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
            var conn = packet.Connection;
            var playerData = new PlayerComponent(networkNode: conn);
            var playerEntity = _playerSys.Add(playerData, 1);

            BodyDefinition bodyDefinition = 
                new BodyDefinition() { Position = new Vector2(40f, 30f) };

            var midgard = LookupSystem<Midgard>();
            var body = midgard.CreateBody(bodyDefinition);
            playerData.Body = body;
            body.UserData = playerEntity;
            var dObject = (DataObject)ObjectMapper.Create((uint)playerEntity.UniqueId, typeof(DataObject));

            {
                var pd = new PlayerComponent(null);
                var pe = midgard.EntityManager.Create(2);
                pe.AddComponent(pd);
                bodyDefinition =
                new BodyDefinition()
                {
                    Position = new Vector2(0f, 0f),
                    LinearVelocity = new Vector2(2f, 2f)             
                };

                body = midgard.CreateBody(bodyDefinition);
                pd.Body = body;
                body.UserData = pe;
                dObject = (DataObject)ObjectMapper.Create((uint)pe.UniqueId, typeof(DataObject));
            }

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
