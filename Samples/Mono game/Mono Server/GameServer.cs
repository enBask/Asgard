using Artemis;
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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mono_Server
{
    public class GameServer : AsgardServer
    {
        BifrostServer _bifrost;
        PlayerSystem _playerSys;
        MonoServer _monoServer;

        public GameServer(MonoServer renderServer)
        {
            _monoServer = renderServer;
            _playerSys = new PlayerSystem();
            AddEntitySystem(_playerSys);
            _bifrost = LookupSystem<BifrostServer>();

            PacketFactory.AddCallback<MonoLoginPacket>(OnLogin);
            PacketFactory.AddCallback<ClientStatePacket>(OnClientState);

            _bifrost.OnDisconnect += _bifrost_OnDisconnect;

            SetupWorld();


        }

        private void _bifrost_OnDisconnect(Asgard.Core.Network.NetNode connection)
        {
            var player = _playerSys.Get(connection);
            if (player != null)
            {
                _playerSys.Remove(player);
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

        private void SetupWorld()
        {
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
            packet.OwnerEntityId = entity.UniqueId;
            _bifrost.Send(packet, obj.Connection);
        }

        protected override void BeforePhysics(float delta)
        {
            base.BeforePhysics(delta);

            var playerEnts = EntityManager.GetEntities(Aspect.All(typeof(PlayerComponent), typeof(Physics2dComponent),
                typeof(RenderData)));
            foreach (var player in playerEnts)
            {
                var playerComp = player.GetComponent<PlayerComponent>();
                var renderData = player.GetComponent<RenderData>();
                var state = playerComp.GetNextState();
                if (state == null) continue;

                var phyComp = player.GetComponent<Physics2dComponent>();

                if (state.LeftMouseDown)
                {
                    renderData.MovingToPosition = true;
                }


                if (renderData.MovingToPosition)
                {
                    var curPos = phyComp.Body.Position;

                    var diff = state.MousePositionInWorld - curPos;
                    if (diff.LengthSquared() <= 0.001)
                    {
                        Trace.WriteLine("stop moving");
                        renderData.MovingToPosition = false;
                        phyComp.Body.LinearVelocity = Vector2.Zero;
                    }
                    else
                    {
                        diff = new Vector2(                            
                            (diff.X != 0f && !float.IsNaN(diff.X)) ? diff.X / Math.Abs(diff.X) : 0f,
                            (diff.Y != 0f && !float.IsNaN(diff.Y)) ? diff.Y / Math.Abs(diff.Y) : 0f
                            );

                        renderData.SetSpeed(player, diff);
                    }
                }

            }
        }

        protected override void AfterPhysics(float delta)
        {
            base.AfterPhysics(delta);

            var playerEnts = EntityManager.GetEntities(Aspect.All(typeof(PlayerComponent), typeof(Physics2dComponent),
                typeof(RenderData)));
            foreach (var player in playerEnts)
            {
                var phyComp = player.GetComponent<Physics2dComponent>();
                var pComp = player.GetComponent<PlayerComponent>();

                if (phyComp.Body == null) continue;
                pComp.RenderPosition = phyComp.Body.Position;
            }
        }

        protected override bool CanPlayerSee(Entity player, Entity checkPlayer)
        {
            var phyComp1 = player.GetComponent<Physics2dComponent>();
            var phyComp2 = checkPlayer.GetComponent<Physics2dComponent>();

            var d = phyComp2.Body.Position - phyComp1.Body.Position;
            var dist = d.LengthSquared();
            return dist < 110f;
        }
    }
}
