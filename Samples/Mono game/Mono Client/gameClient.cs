using System;
using Artemis;
using Asgard.Client;
using Asgard.Core.Network.Packets;
using Asgard.Core.System;
using Asgard.EntitySystems.Components;
using Farseer.Framework;
using Microsoft.Xna.Framework.Content;
using Shared;
using Asgard;
using Asgard.Core.Network;
using System.Diagnostics;
using System.Collections.Generic;
using Asgard.Core.Physics;

namespace Mono_Client
{
    public class gameClient : AsgardClient
    {
        public ContentManager Content;

        private Entity _thisPlayer;

        public PlayerState CurrentState { get; set; }

        public gameClient()
        {
            CurrentState = new PlayerState(null);

            PacketFactory.AddCallback<LoginResponsePacket>(onLogin);

            var bifrost = LookupSystem<BifrostClient>();
            bifrost.OnConnection += Bifrost_OnConnection;
            bifrost.Start();

        }

        public RenderData GetPlayerData()
        {
            if (_thisPlayer == null) return null;
            return _thisPlayer.GetComponent<RenderData>();
        }

        private void Bifrost_OnConnection(Asgard.Core.Network.NetNode connection)
        {
            var bifrost = LookupSystem<BifrostClient>();
            MonoLoginPacket packet = new MonoLoginPacket();
            bifrost.Send(packet);
        }

        private void onLogin(LoginResponsePacket packet)
        {
            var id = packet.OwnerEntityId;
            var entity = EntityManager.GetEntityByUniqueId(id);
            if (entity == null)
            {
                entity = ObjectMapper.CreateEntityById(id);
            }
            PlayerComponent pComp = new PlayerComponent(null);
            entity.AddComponent(pComp);
            _thisPlayer = entity;
        }

        protected override void BeforePhysics(float delta)
        {
            base.BeforePhysics(delta);

            var renderData = GetPlayerData();
            if (renderData == null) return;
            if (CurrentState.LeftMouseDown)
            {
                renderData.MovingToPosition = true;
            }

            if (renderData.MovingToPosition)
            {
                var phyComp = _thisPlayer.GetComponent<Physics2dComponent>();
                var curPos = phyComp.Body.Position;

                var diff = CurrentState.MousePositionInWorld - curPos;
                if (diff.LengthSquared() <= 0.001)
                {
                    renderData.MovingToPosition = false;
                    phyComp.Body.LinearVelocity = Vector2.Zero;
                }
                else
                {
                    diff = new Vector2(
                        (diff.X != 0f && !float.IsNaN(diff.X)) ? diff.X / Math.Abs(diff.X) : 0f,
                        (diff.Y != 0f && !float.IsNaN(diff.Y)) ? diff.Y / Math.Abs(diff.Y) : 0f
                        );

                    var renderComp = _thisPlayer.GetComponent<RenderData>();
                    renderComp.SetSpeed(_thisPlayer, diff);
                }
            }
        }

        protected override void AfterPhysics(float delta)
        {
            base.AfterPhysics(delta);

            if (_thisPlayer == null) return;

            var phyComp = _thisPlayer.GetComponent<Physics2dComponent>();
            if (phyComp == null || phyComp.Body == null) return;

            PlayerState newState = new PlayerState(phyComp)
            {
                LeftMouseDown = CurrentState.LeftMouseDown,
                MousePositionInWorld = CurrentState.MousePositionInWorld
            };

            AddClientState(newState);
        }

        protected override Entity GetPlayer()
        {
            return _thisPlayer;
        }

    }
}
