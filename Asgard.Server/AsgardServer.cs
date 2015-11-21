using Asgard.Server.Network;
using Asgard.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis;
using Artemis.System;
using Asgard.Core.Network;

namespace Asgard
{
    public class AsgardServer
    {
        public delegate void OnSendSnapshotHandler();
        public event OnSendSnapshotHandler OnSendSnapShot;


        #region Private Vars
        Dictionary<Type, ISystem> _systems = new Dictionary<Type, ISystem>();
        SortedList<int, List<ISystem>> _sortedSystems = new SortedList<int, List<ISystem>>();
        BifrostServer _bifrost = null;
        EntityWorld _entityWorld;
        #endregion

        public AsgardServer()
        {
            _entityWorld = new EntityWorld(false, true, true);

            LoadConfig();
        }

        private void LoadConfig()
        {
            NetConfig netConfig = Config.Get<NetConfig>("network");
            _bifrost = new BifrostServer(netConfig.Port, netConfig.MaxConnections);
            AddInternalSystem<BifrostServer>(_bifrost, 0);

            var physics = new PhysicsSystem2D();
            AddEntitySystem<PhysicsSystem2D>(physics);
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

            List<ISystem> systems;
            if (!_sortedSystems.TryGetValue(runOrder, out systems))
            {
                systems = new List<ISystem>();
                _sortedSystems.Add(runOrder, systems);
            }
            systems.Add(system);
        }

        public void AddSystem<T>(T system, int runOrder=1) where T : class, ISystem
        {
            if (runOrder < 1)
            {
                throw new ArgumentException("invalid runOrder");
            }

            AddInternalSystem<T>(system, runOrder);
        }

        public void AddEntitySystem<T>(T system, int runOrder=1) where T : EntitySystem, ISystem
        {
            if(runOrder < 1)
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

        public T LookupSystem<T>() where T: class,ISystem
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



        public void Run()
        {
            foreach(var runOrder in _sortedSystems.Values)
            {
                foreach(var system in runOrder)
                {
                    system.Start();
                }
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();
            float lastSecondStamp = (float)timer.Elapsed.TotalSeconds;
            while (true)
            {
                System.Threading.Thread.Sleep(30);
                var elapsedSeconds = (float)timer.Elapsed.TotalSeconds;
                var delta = elapsedSeconds - lastSecondStamp;
                lastSecondStamp = elapsedSeconds;

                foreach (var runOrder in _sortedSystems.Values)
                {
                    foreach (var system in runOrder)
                    {
                        system.Tick(delta);
                    }
                }

                _entityWorld.Update();

                if (OnSendSnapShot != null)
                {
                    OnSendSnapShot();
                }

                System.Threading.Thread.Sleep(1);
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
