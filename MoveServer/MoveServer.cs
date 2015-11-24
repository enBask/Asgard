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

namespace MoveServer
{
    public class MoveServer : AsgardServer<SnapshotPacket, MoveData>
    {
        BifrostServer _bifrost;
        PlayerSystem<PlayerData> _playerSys;
        int _worldId;

        public MoveServer()
        {
            _playerSys = new PlayerSystem<PlayerData>();
            AddEntitySystem(_playerSys);

            PacketFactory.AddCallback<MoveLoginPacket>(OnLogin);
        }


        private void OnLogin(MoveLoginPacket packet)
        {
            var conn = packet.Connection;
            var playerEntity = _playerSys.Add(new PlayerData(networkNode: conn));

            var phyData = new Physics2dComponent();
            phyData.WorldID = _worldId;
            phyData.StartingPosition = new Vector2(40f, 30f);
            phyData.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
            phyData.StartingRestitution = 1.0f;

            var shape = new CircleShape(1f, 0.001f);
            phyData.Shapes.Add(shape);

            playerEntity.AddComponent(phyData);
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

        protected override List<MoveData> GetPlayerDataView(int entityId)
        {
            List<MoveData> moveDataSet = new List<MoveData>();

            Bag<Entity> players = _playerSys.EntityManager.GetEntities(Aspect.One(typeof(PlayerData)));
            foreach(var player in players)
            {
                var phyData = player.GetComponent<Physics2dComponent>();
                if (phyData == null || phyData.Body == null) continue;

                var moveData = new MoveData(phyData.Body.Position.X, phyData.Body.Position.Y, 0f, 0f);
                moveData.Id = entityId;

                moveDataSet.Add(moveData);
            }

            return moveDataSet;
        }

    }
}
