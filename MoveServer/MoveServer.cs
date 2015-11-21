using Asgard;
using Asgard.Core.Network.Packets;
using Asgard.EntitySystems;
using ChatServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Manager;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Asgard.EntitySystems.Components;

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
            phyData.StartingPosition = new Vector2(0f, 0f);
            phyData.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;

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


        public bool Start()
        {
            _bifrost = LookupSystem<BifrostServer>();
            _bifrost.OnDisconnect += _bifrost_OnDisconnect;

            var physicsSys = LookupSystem<PhysicsSystem2D>();
            _worldId = physicsSys.CreateWorld(new Vector2(0f, 0f));

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

            uint snap_id = (uint)Math.Floor(_bifrost.NetTime * 30.0);

            var snapPacket = new SnapshotPacket();
            snapPacket.Id = snap_id;
            snapPacket.DataPoints = new List<MoveData>();
            var connections = _playerSys.Connections;
            if (connections.Count == 0) return;

            foreach (var connection in connections)
            {
                var playerEntity = _playerSys.Get(connection);
                var phyComp = playerEntity.GetComponent<Physics2dComponent>();

                if (phyComp != null)
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
