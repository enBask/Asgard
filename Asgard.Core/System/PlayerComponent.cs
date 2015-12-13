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
using System.Diagnostics;

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

        private Dictionary<NetworkObject, uint> _knownObjects;

        private Dictionary<int, DeltaList> _deltaBuffer;

        public PlayerComponent(NetNode networkNode)
        {
            NetworkNode = networkNode;
            InputBuffer = new JitterBuffer<PlayerStateData>(30);
            _knownObjects = new Dictionary<NetworkObject, uint>();
            _deltaBuffer = new Dictionary<int, DeltaList>();
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
        internal void AddKnownObject(NetworkObject obj, uint eId)
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

        internal List<Tuple<NetworkObject, uint>> FindDeletedObjects(List<NetworkObject> fullList)
        {
            //get the known object list as a simple collection
            var objList = _knownObjects.Keys.AsEnumerable();

            //cross the two lists to get the missing set.
            var missingItems = objList.Except(fullList);
            var retList = missingItems.Select(n =>
            {
                var id = _knownObjects[n];
                return new Tuple<NetworkObject, uint>(n, id);
            });

            return retList.ToList();
        }

        internal void RemoveKnownObject(NetworkObject obj)
        {
            if (_knownObjects.ContainsKey(obj))
                _knownObjects.Remove(obj);
        }
        #endregion

        
        internal void AddDeltaState(List<DeltaLookup> syncList)
        {
            foreach (var lookup in syncList)
            {
                DeltaList objList;
                if (!_deltaBuffer.TryGetValue(lookup.Lookup, out objList))
                {
                    _deltaBuffer[lookup.Lookup] = objList = 
                        new DeltaList();
                }

                objList.Objects.AddToTail(new DeltaWrapper()
                    {
                        Lookup = NetTime.SimTick,
                        Object = lookup.Object
                    });
            }

        }

        internal DeltaWrapper FindBaseline(NetworkObject baseObj)
        {

            DeltaList objList;
            if (_deltaBuffer.TryGetValue(baseObj.GetHashCode(), out objList))
            {
                if (!objList.HasAcked) return null;

                var head = objList.Objects.First;
                if (head != null)
                    return head.Value;
            }
            return null;
        }

        internal void AckDeltaBaseline(uint simTick)
        {
            foreach(var list in _deltaBuffer.Values)
            {
                foreach(var node in list.Objects)
                {
                    if (node.Value.Lookup == simTick)
                    {
                        list.HasAcked = true;
                        list.Objects.TruncateTo(node);
                        break;
                    }
                }
            }
        }
    }
}
