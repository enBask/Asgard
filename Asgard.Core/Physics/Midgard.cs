using Artemis;
using Asgard.Core.Network;
using Asgard.Core.System;
using Asgard.EntitySystems.Components;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Farseer.Framework;
using System.Collections.Generic;
using FarseerPhysics.Factories;

namespace Asgard.Core.Physics
{
    public class Midgard : BaseSystem
    {
        public delegate void TickCallback(float delta);
        public event TickCallback OnBeforeTick;
        public event TickCallback OnAfterTick;

        List<Body> _queueDeleteList = new List<Body>();
        World _world;
        int _tickRate;
        float _invTickRate;
        double _accum;

        public Midgard(AABB boundBox, Vector2 gravity, int tickRate)
        {
            _tickRate = tickRate;
            _invTickRate = 1f / (float)_tickRate;

            FarseerPhysics.Settings.ContinuousPhysics = false;

            _world = new World(gravity);

        }

        public List<Body> BodyList
        {
            get
            {
                return _world.BodyList;
            }
        }

        public Body CreateBody(BodyDefinition definition)
        {
            var body = new Body(_world, definition.Position, definition.Angle);
            body.BodyType = BodyType.Dynamic;
            body.LinearVelocity = definition.LinearVelocity;
            return body;
        }

        public void DeleteBody(Physics2dComponent comp)
        {
            if (comp != null && comp.Body != null)
            {
                _queueDeleteList.Add(comp.Body);
            }
        }

        public World GetWorld()
        {
            return _world;
        }

        public Physics2dComponent CreateComponent(Entity entity, BodyDefinition definition, bool remoteSync = true)
        {
            var body = CreateBody(definition);

            var component = new Physics2dComponent();
            component.Body = body;
            body.UserData = entity;
            entity.AddComponent(component);


            if (remoteSync)
            {
                ObjectMapper.Create(
                    (uint)entity.UniqueId, typeof(NetPhysicsObject));
            }

            return component;
        }

        public Body LookupBody(Entity entity)
        {
            var comp = entity.GetComponent<Physics2dComponent>();
            if (comp == null)
            {
                return null;
            }
            return comp.Body;
        }

        public void Step(float delta)
        {
            if (OnBeforeTick != null)
            {
                OnBeforeTick(delta);
            }

            foreach(var body in _queueDeleteList)
            {
                _world.RemoveBody(body);
            }
            _queueDeleteList.Clear();

            _world.Step(delta);
            NetTime.SimTick++;

            if (OnAfterTick != null)
            {
                OnAfterTick(delta);
            }
        }

        public override void Tick(double delta)
        {
            _accum += delta;
            if (_accum >= _invTickRate)
            {
                while (_accum >= _invTickRate)
                {
                    _accum -= _invTickRate;
                    Step(_invTickRate);
                }
            }
        }
    }
}
