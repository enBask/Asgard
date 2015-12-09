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
        private Core.Collections.LinkedList<Tuple<uint, Dictionary<int, NetworkObject>>> _deltaBuffer;
            

        public PlayerComponent(NetNode networkNode)
        {
            NetworkNode = networkNode;
            InputBuffer = new JitterBuffer<PlayerStateData>(30);
            _knownObjects = new Dictionary<NetworkObject, long>();
            _deltaBuffer = 
                new Core.Collections.LinkedList<Tuple<uint, Dictionary<int, NetworkObject>>>();
        }

        public PlayerStateData GetNextState()
        {
            var states = InputBuffer.Get();
            if (states != null)
            {
                CurrentState = states;
            }

            return CurrentState;
        }

        #region object tracking for remote create/delete
        internal void AddKnownObject(NetworkObject obj, long eId)
        {
            if (!IsObjectKnown(obj))
            {
                _knownObjects.Add(obj, eId);
            }
        }

        internal bool IsObjectKnown(NetworkObject obj)
        {
            return _knownObjects.ContainsKey(obj);
        }

        internal List<Tuple<NetworkObject,long>> FindDeletedObjects(List<NetworkObject> fullList)
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

        internal void RemoveKnownObject(NetworkObject obj)
        {
            if (_knownObjects.ContainsKey(obj))
                _knownObjects.Remove(obj);
        }
        #endregion

        internal void AddDeltaState(Dictionary<int, NetworkObject> syncState)
        {
            var deltaState = new Tuple<uint, Dictionary<int, NetworkObject>>
                (
                NetTime.SimTick,
                syncState
                );
            _deltaBuffer.AddToTail(deltaState);
        }

        internal Tuple<uint, Dictionary<int, NetworkObject>> GetDeltaBaseline()
        {
            if (_deltaBuffer.First == null) return null;
            return _deltaBuffer.First.Value;
        }

        internal void AckDeltaBaseline(uint simTick)
        {
            foreach(var node in _deltaBuffer)
            {
                if (node.Value.Item1 == simTick)
                {
                    _deltaBuffer.TruncateTo(node);
                    return;
                }
            }
        }

    }
}
