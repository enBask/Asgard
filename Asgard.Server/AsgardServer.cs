using Asgard.Network;
using Asgard.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard
{
    public class AsgardServer
    {
        Dictionary<string, ISystem> _systems = new Dictionary<string, ISystem>();
        SortedList<int, ISystem> _sortedSystems = new SortedList<int, ISystem>();

        BifrostServer _bifrost = null;

        public AsgardServer()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            NetConfig netConfig = Config.Get<NetConfig>("network");
            _bifrost = new BifrostServer(netConfig.Port, netConfig.MaxConnections);
            AddInternalSystem("bifrost", _bifrost, 0);
        }

        internal void AddInternalSystem(string name, ISystem system, int runOrder = 1)
        {
            _systems[name] = system;
            _sortedSystems.Add(runOrder, system);
        }

        public void AddSystem(string name, ISystem system, int runOrder=1)
        {
            if (runOrder < 1)
            {
                throw new ArgumentException("invalid runOrder");
            }

            _systems[name] = system;
            _sortedSystems.Add(runOrder, system);
        }

        public ISystem LookupSystem(string name)
        {
            ISystem sys;
            if (_systems.TryGetValue(name , out sys))
            {
                return sys;
            }

            return null;
        }

        public void Run()
        {
            foreach(var system in _sortedSystems.Values)
            {
                system.Start();
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();
            double lastSecondStamp = timer.Elapsed.TotalSeconds;
            while (true)
            {
                var elapsedSeconds = timer.Elapsed.TotalSeconds;
                var delta = elapsedSeconds - lastSecondStamp;
                lastSecondStamp = elapsedSeconds;

                foreach (var system in _sortedSystems.Values)
                {
                    system.Tick(delta);
                }

//                _bifrost.SendFrame();

                System.Threading.Thread.Sleep(1);
            }


            foreach (var system in _sortedSystems.Values)
            {
                system.Stop();
            }
        }
    }
}
