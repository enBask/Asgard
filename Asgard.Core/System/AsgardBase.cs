﻿using Artemis;
using Artemis.System;
using Asgard.Core.Network.Packets;
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
        #region prviate vars
        Dictionary<Type, ISystem> _systems = new Dictionary<Type, ISystem>();
        SortedList<int, List<ISystem>> _sortedSystems = new SortedList<int, List<ISystem>>();
        EntityWorld _entityWorld;
        #endregion

        static AsgardBase()
        {
            Network.Bootstrap.Init();
        }

        public AsgardBase()
        {
            _entityWorld = new EntityWorld(false, true, true);
            ObjectMapper.Init(_entityWorld.EntityManager);
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
        #endregion

        protected virtual void Tick(double delta)
        {

        }

        protected virtual void BeforeTick(double delta)
        {

        }

        public virtual void Run()
        {
            foreach (var runOrder in _sortedSystems.Values)
            {
                foreach (var system in runOrder)
                {
                    system.Start();
                }
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();
            double lastSecondStamp = timer.Elapsed.TotalSeconds;
            while (true)
            {
                double elapsedSeconds = timer.Elapsed.TotalSeconds;
                double delta = elapsedSeconds - lastSecondStamp;
                lastSecondStamp = elapsedSeconds;

                BeforeTick(delta);

                foreach (var runOrder in _sortedSystems.Values)
                {
                    foreach (var system in runOrder)
                    {
                        system.Tick(delta);
                    }
                }
                _entityWorld.Update();

                Tick(delta);

                Thread.Sleep(1);
            }

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
