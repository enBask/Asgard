using Artemis.Interface;
using Asgard.Core.Collections;
using Asgard.Core.Network;
using Asgard.Core.Network.Data;
using Asgard.Core.System;
using FarseerPhysics.Dynamics;
using Farseer.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Asgard.EntitySystems.Components
{
    public class PlayerComponent : IComponent
    {
        public NetNode NetworkNode { get; set; }

        public JitterBuffer<PlayerStateData> InputBuffer { get; set; }
        public PlayerStateData CurrentState { get; set; }
        public Vector2 OldPosition { get; set; }
        public Vector2 RenderPosition { get; set; }
        public bool LerpToReal { get; set; }
        public float LerpStart { get; set; }
        public float LerpEnd { get; set; }

        private Dictionary<NetworkObject, long> _knownObjects;

        public PlayerComponent(NetNode networkNode)
        {
            NetworkNode = networkNode;
            InputBuffer = new JitterBuffer<PlayerStateData>(30);
            _knownObjects = new Dictionary<NetworkObject, long>();
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

        public void AddKnownObject(NetworkObject obj, long eId)
        {
            if (!IsObjectKnown(obj))
            {
                _knownObjects.Add(obj, eId);
            }
        }

        public bool IsObjectKnown(NetworkObject obj)
        {
            return _knownObjects.ContainsKey(obj);
        }

        public List<Tuple<NetworkObject,long>> FindDeletedObjects(List<NetworkObject> fullList)
        {
            //get the known object list as a simple collection
            var objList = _knownObjects.Keys.AsEnumerable();

            //cross the two lists to get the missing set.
            var missingItems = objList.Except(fullList);
            var retList = missingItems.Select(n =>
            {
                var id = _knownObjects[n];
                return new Tuple<NetworkObject, long>(n, id);
            });

            return retList.ToList();
        }

        public void RemoveKnownObject(NetworkObject obj)
        {
            if (_knownObjects.ContainsKey(obj))
                _knownObjects.Remove(obj);
        }
    }
}
