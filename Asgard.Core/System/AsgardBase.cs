using Artemis;
using Artemis.Manager;
using Artemis.System;
using Asgard.Core.Network.Packets;
using Asgard.Core.Physics;
using FarseerPhysics.Collision;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Asgard.Core.System
{
    public class AsgardBase
    {
        #region private vars
        Dictionary<Type, ISystem> _systems = new Dictionary<Type, ISystem>();
        SortedList<int, List<ISystem>> _sortedSystems = new SortedList<int, List<ISystem>>();
        EntityWorld _entityWorld;
        Midgard _midgard;
        #endregion

        static AsgardBase()
        {
            Network.Bootstrap.Init();
        }

        public AsgardBase()
        {
            _entityWorld = new EntityWorld(false, true, true);
            ObjectMapper.Init(this);

            AABB bounds = new AABB(new Vector2(40f, 30f), 85f, 65f);
            _midgard = new Midgard(bounds, new Vector2(0,0), 60);
            AddInternalSystem(_midgard, 0);

            _midgard.OnBeforeTick += BeforePhysics;
            _midgard.OnAfterTick += AfterPhysics;
        }

        public EntityManager EntityManager
        {
            get
            {
                return _entityWorld.EntityManager;
            }
        }

        #region system system
        internal void AddInternalSystem<T>(T system, int runOrder = 1) where T : class, ISystem
        {
            var type = typeof(T);

            if (system is BaseSystem)
            {
                (system as BaseSystem).EntityManager = _entityWorld.EntityManager;
            }

            _systems[type] = system;
            system.Base = this;

            List<ISystem> systems;
            if (!_sortedSystems.TryGetValue(runOrder, out systems))
            {
                systems = new List<ISystem>();
                _sortedSystems.Add(runOrder, systems);
            }
            systems.Add(system);
        }

        public void AddSystem<T>(T system, int runOrder = 1) where T : class, ISystem
        {
            if (runOrder < 1)
            {
                throw new ArgumentException("invalid runOrder");
            }

            AddInternalSystem<T>(system, runOrder);
        }

        public void AddEntitySystem<T>(T system, int runOrder = 1) where T : EntitySystem, ISystem
        {
            if (runOrder < 1)
            {
                throw new ArgumentException("invalid runOrder");
            }

            AddInternalSystem<T>(system, runOrder);
            _entityWorld.SystemManager.SetSystem<T>(system, Artemis.Manager.GameLoopType.Update);
        }

        public T GetEntitySystem<T>() where T : EntitySystem
        {
            return _entityWorld.SystemManager.GetSystem<T>();
        }

        public T LookupSystem<T>() where T : class, ISystem
        {
            ISystem sys;
            var type = typeof(T);
            if (_systems.TryGetValue(type, out sys))
            {
                return sys as T;
            }

            return null;
        }

        public ISystem LookupSystem(Type type)
        {
            ISystem sys;
            if (_systems.TryGetValue(type, out sys))
            {
                return sys;
            }

            return null;
        }
        #endregion

        public  void Tick(double delta)
        {

            BeforeTick(delta);

            foreach (var runOrder in _sortedSystems.Values)
            {
                foreach (var system in runOrder)
                {
                    system.Tick(delta);
                }
            }
            _entityWorld.Update();

            AfterTick(delta);

        }

        protected virtual void BeforeTick(double delta)
        {

        }

        protected virtual void AfterTick(double delta)
        {

        }


        protected virtual void BeforePhysics(float delta)
        {

        }

        protected virtual void AfterPhysics(float delta)
        {
        }

        public virtual void Start()
        {
            foreach (var runOrder in _sortedSystems.Values)
            {
                foreach (var system in runOrder)
                {
                    system.Start();
                }
            }
        }

        public virtual void Stop()
        {
            foreach (var runOrder in _sortedSystems.Values)
            {
                foreach (var system in runOrder)
                {
                    system.Stop();
                }
            }
        }
    }
}
