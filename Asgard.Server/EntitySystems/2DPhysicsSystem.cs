using Artemis.System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Manager;
using Asgard.EntitySystems.Components;
using Artemis;
using FarseerPhysics.Factories;

namespace Asgard
{
    public class PhysicsSystem2D : EntityComponentProcessingSystem<Physics2dComponent>, ISystem
    {
        float _stepTime = 0f;
        ConcurrentDictionary<int, World> _Worlds = new ConcurrentDictionary<int, World>();
        int _currentWorldId=0;
        float _deltaTime = 0f;

        public EntityManager EntityManager
        {
            get
            {
                return EntityWorld.EntityManager;
            }
        }

        public PhysicsSystem2D(float steptime = 0.03f) // 30 TPS
        {
            _stepTime = steptime;
            _Worlds = new ConcurrentDictionary<int, World>();
        }

        public bool Start()
        {
            EntityManager.RemovedComponentEvent += EntityManager_RemovedComponentEvent;
            return true;
        }

        public int CreateWorld(Vector2 grav)
        {
            var  id = _currentWorldId++;
            var world = new World(grav);
            _Worlds[id] = world;
            return id;
        }

        public bool DeleteWorld(int id)
        {
            World world;
            if (_Worlds.TryGetValue(id, out world))
            {
                world.Clear();
                world.ClearForces();
                return _Worlds.TryRemove(id, out world);
            }

            return false;
        }

        public World GetWorld(int id)
        {
            World world;
            _Worlds.TryGetValue(id, out world);
            return world;
        }



        public void Tick(float delta)
        {
            _deltaTime += delta;

            if (_deltaTime < _stepTime)
                return;

            int ticks = 0;
            while(_deltaTime >= _stepTime)
            {
                ticks++;
                _deltaTime -= _stepTime;
            }

            foreach (var world in _Worlds.Values)
            {
                for(int i =0; i < ticks; ++i)
                    world.Step(delta);
            }
        }

        public bool Stop()
        {
            return true;
        }


        #region Component System
        public override void OnAdded(Entity entity)
        {
            var pComp = entity.GetComponent<Physics2dComponent>();
            if (pComp != null)
            {
                var wId = pComp.WorldID;
                var world = GetWorld(wId);
                if (world != null)
                {
                    var body = BodyFactory.CreateBody(world, pComp.StartingPosition, 0, entity);
                    body.BodyType = pComp.BodyType;
                    pComp.Body = body;

                    foreach(var shape in pComp.Shapes)
                    {
                        var fixture = body.CreateFixture(shape);
                        fixture.Restitution = pComp.StartingRestitution;
                    }
                }
            }
        }

        private void EntityManager_RemovedComponentEvent(Entity entity, Artemis.Interface.IComponent component)
        {
            var pComp = (component as Physics2dComponent);
            if (pComp != null)
            {
                var world = GetWorld(pComp.WorldID);
                if (world != null)
                {
                    if (pComp.Body != null)
                    {
                        world.RemoveBody(pComp.Body);
                        pComp.Body = null;
                    }
                }
            }
        }



        public override void Process(Entity entity, Physics2dComponent component1)
        {
        }
        #endregion
    }
}
