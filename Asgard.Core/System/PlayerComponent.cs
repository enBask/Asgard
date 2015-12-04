using Artemis.Interface;
using Asgard.Core.Collections;
using Asgard.Core.Network;
using Asgard.Core.Network.Data;
using Asgard.Core.System;
using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;

namespace Asgard.EntitySystems.Components
{
    public class PlayerComponent : IComponent
    {
        public NetNode NetworkNode { get; set; }

        public JitterBuffer<PlayerStateData> InputBuffer { get; set; }
        public PlayerStateData CurrentState { get; set; }

        private List<WeakReference<NetworkObject>> _knownObjects { get; set; }

        public PlayerComponent(NetNode networkNode)
        {
            NetworkNode = networkNode;
            InputBuffer = new JitterBuffer<PlayerStateData>(30);
            _knownObjects = new List<WeakReference<NetworkObject>>();
        }

        public PlayerStateData GetNextState()
        {
            var states = InputBuffer.Get();
            if (states != null && states.Count > 0)
            {
                CurrentState = states[0];
            }

            return CurrentState;
        }

        public void AddKnownObject(NetworkObject obj)
        {
            if (!IsObjectKnown(obj))
            {
                WeakReference<NetworkObject> weakRef = new WeakReference<NetworkObject>(obj);
                _knownObjects.Add(weakRef);
            }
        }

        public bool IsObjectKnown(NetworkObject obj)
        {
            bool bFound = false;
            List<WeakReference<NetworkObject>> outOfScope = new List<WeakReference<NetworkObject>>();
            foreach(var weakRef in _knownObjects)
            {
                NetworkObject netObj;
                weakRef.TryGetTarget(out netObj);
                if (netObj == null)
                {
                    outOfScope.Add(weakRef);
                    continue;
                }
                else if (netObj == obj)
                {
                    bFound = true;
                    break;
                }
            }

            foreach (var scope in outOfScope)
                _knownObjects.Remove(scope);

            return bFound;
            
        }
    }
}
