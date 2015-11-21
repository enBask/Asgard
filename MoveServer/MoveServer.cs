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

namespace MoveServer
{
    public class MoveServer : AsgardServer, ISystem
    {
        BifrostServer _bifrost;
        PlayerSystem<PlayerData> _playerSys;
        int _worldId;

        public EntityManager EntityManager
        {
            get; set;
        }

        public MoveServer()
        {
            _playerSys = new PlayerSystem<PlayerData>();
            AddEntitySystem(_playerSys);
            AddSystem<MoveServer>(this);

            PacketFactory.AddCallback<MoveLoginPacket>(OnLogin);

            OnSendSnapShot += MoveServer_OnSendSnapShot;
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

        public void Tick(float delta)
        {

        }

        private void MoveServer_OnSendSnapShot()
        {

            uint snap_id = (uint)Math.Floor(_bifrost.NetTime * _netConfig.tickrate);

            var snapPacket = new SnapshotPacket();
            snapPacket.Id = snap_id;
            snapPacket.DataPoints = new List<MoveData>();
            var connections = _playerSys.Connections;
            if (connections.Count == 0) return;

            foreach (var connection in connections)
            {
                var playerEntity = _playerSys.Get(connection);
                var phyComp = playerEntity.GetComponent<Physics2dComponent>();


                if (phyComp != null && phyComp.Body != null)
                {
                    var movedata = new MoveData(phyComp.Body.Position.X, phyComp.Body.Position.Y, 0f, 0f);
                    movedata.Id = (ushort)playerEntity.Id;
                    snapPacket.DataPoints.Add(movedata);
                }

            }

            _bifrost.Send(snapPacket, connections.ToList());
        }

    }
}
