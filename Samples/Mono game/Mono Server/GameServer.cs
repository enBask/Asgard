using Asgard;
using Asgard.Core.Network.Packets;
using Asgard.Core.Physics;
using Asgard.Core.System;
using Asgard.EntitySystems;
using Asgard.EntitySystems.Components;
using Farseer.Framework;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using RogueSharp;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono_Server
{
    public class GameServer : AsgardServer
    {
        BifrostServer _bifrost;
        PlayerSystem _playerSys;

        IMap _gameMap;

        MonoServer _monoServer;

        public GameServer(MonoServer renderServer)
        {
            _monoServer = renderServer;
            _playerSys = new PlayerSystem();
            AddEntitySystem(_playerSys);
            _bifrost = LookupSystem<BifrostServer>();

            PacketFactory.AddCallback<MonoLoginPacket>(OnLogin);
            //            PacketFactory.AddCallback<ClientStatePacket>(OnClientState);


            SetupWorld();


        }

        private void SetupWorld()
        {
            IMapCreationStrategy<Map> mapCreationStrategy =
                new RandomRoomsMapCreationStrategy<Map>(50, 50, 3, 20, 10);
            _gameMap = Map.Create(mapCreationStrategy);
        }

        private void OnLogin(MonoLoginPacket obj)
        {
            var midgard = LookupSystem<Midgard>();

            var playerComponent = new PlayerComponent(obj.Connection);
            var entity = _playerSys.Add(playerComponent);

            RenderData renderData = (RenderData)ObjectMapper.Create(entity.UniqueId, typeof(RenderData));
            renderData.Set(midgard, entity, _monoServer.Content);

            entity.AddComponent(renderData);

            LoginResponsePacket packet = new LoginResponsePacket();
            _bifrost.Send(packet, obj.Connection);
        }

    }
}
