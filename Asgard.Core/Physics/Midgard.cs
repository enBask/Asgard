using Artemis;
using Asgard.EntitySystems.Components;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Asgard.Core.Physics
{
    public class Midgard : BaseSystem
    {
        public delegate void TickCallback(float delta);
        public event TickCallback OnBeforeTick;
        public event TickCallback OnAfterTick;

        World _world;
        int _tickRate;
        float _invTickRate;
        double _accum;

        public Midgard(AABB boundBox, Vector2 gravity, int tickRate)
        {
            _tickRate = tickRate;
            _invTickRate = 1f / (float)_tickRate;

            _world = new World(gravity, boundBox);

        }

        public Body CreateBody(BodyDefinition definition)
        {
            var body = new Body(_world, definition.Position, definition.Angle);
            body.BodyType = BodyType.Dynamic;
            body.LinearVelocity = definition.LinearVelocity;
            return body;
        }

        public Physics2dComponent CreateComponent(Entity entity, BodyDefinition definition)
        {
            var body = CreateBody(definition);

            var component = new Physics2dComponent();
            component.Body = body;
            body.UserData = entity;
            entity.AddComponent(component);
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

            _world.Step(delta);


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
