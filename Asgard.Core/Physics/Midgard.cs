using Artemis;
using Asgard.EntitySystems.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Physics
{
    public class Midgard : BaseSystem
    {
        AABB _boundingBox;
        List<Body> _bodies;

        int _tickRate;
        float _invTickRate;
        double _accum;
        public Midgard(AABB boundBox, int tickRate)
        {
            _tickRate = tickRate;
            _invTickRate = 1f / (float)_tickRate;

            _boundingBox = boundBox;
            _bodies = new List<Body>();
        }

        public Body CreateBody(BodyDefinition definition)
        {
            var body = new Body(definition);
            _bodies.Add(body);
            return body;
        }

        public void Step(float delta)
        {
            IntegrateInputs();

            IntegrateVelocities(delta);

            IntegrateCollisions();

        }

        private void IntegrateInputs()
        {
            var players = EntityManager.GetEntities(Aspect.One(typeof(PlayerComponent)));
            foreach(var player in players)
            {
                var pComponent = player.GetComponent<PlayerComponent>();
                if (pComponent.Body == null) continue;

                var stateData = pComponent.GetNextState();
                if (stateData == null) continue;

                float speed = 25f;
                Vector2 vel = new Vector2();
                if (stateData.Forward)
                {
                    vel.Y = -speed;
                }
                if (stateData.Back)
                {
                    vel.Y = speed;
                }

                if (stateData.Right)
                {
                    vel.X = speed;
                }
                if (stateData.Left)
                {
                    vel.X = -speed;
                }

                pComponent.Body.LinearVelocity = vel;

            }
        }

        private void IntegrateCollisions()
        {
            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];

                if (!_boundingBox.PointIn(body._position))
                {
                    var min = _boundingBox.Min();
                    var max = _boundingBox.Max();

                    if (body._position.X > max.X)
                        body._position.X = max.X;
                    else if (body._position.X < min.X)
                        body._position.X = min.X;

                    if (body._position.Y > max.Y)
                        body._position.Y = max.Y;
                    else if (body._position.Y < min.Y)
                        body._position.Y = min.Y;

                    body._linearVelocity = Vector2.Zero;
                    body._sleeping = true;
                }
            }
        }

        private void IntegrateVelocities(float delta)
        {
            for(int i =0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];

                body._position += body._linearVelocity * delta;            
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
