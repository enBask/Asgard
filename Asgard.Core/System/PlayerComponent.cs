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
using Artemis;

namespace Asgard.EntitySystems.Components
{
    class AccumulatorEntity : IComparable<AccumulatorEntity>
    {
        public Entity Entity;
        public int Score;

        public AccumulatorEntity(Entity e, int s)
        {
            Entity = e;
            Score = s;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals((AccumulatorEntity)obj, null))
                return false;

            return Entity.Equals(((AccumulatorEntity)obj).Entity);
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode();
        }
    
        public int CompareTo(AccumulatorEntity other)
        {
            if (ReferenceEquals(other, this)) return 0;

            if (other == null) return -1;

            return Score.CompareTo(other.Score);
        }
}

    public class PlayerComponent : IComponent
    {
        public NetNode NetworkNode { get; set; }

        public JitterBuffer<PlayerStateData> InputBuffer { get; set; }
        public PlayerStateData CurrentState { get; set; }
        public PlayerStateData LastSyncState { get; set; }
        public Vector2 OldPosition { get; set; }
        public Vector2 RenderPosition { get; set; }
        public bool LerpToReal { get; set; }
        public float LerpStart { get; set; }
        public float LerpEnd { get; set; }

        private Dictionary<NetworkObject, uint> _knownObjects;
        private Dictionary<int, DeltaList> _deltaBuffer;
        private Dictionary<Entity, int> _accumBuffer;
        public PlayerComponent(NetNode networkNode)
        {
            NetworkNode = networkNode;
            InputBuffer = new JitterBuffer<PlayerStateData>(30);
            InputBuffer.Id = 1;
            _knownObjects = new Dictionary<NetworkObject, uint>();
            _deltaBuffer = new Dictionary<int, DeltaList>();
            _accumBuffer = new Dictionary<Entity, int>();
        }

        internal PlayerStateData GetNextState()
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


        #region delta state tracking
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

        internal void RemoveDeltaTrack(NetworkObject obj)
        {
            if (_deltaBuffer.ContainsKey(obj.GetHashCode()))
            {
                _deltaBuffer.Remove(obj.GetHashCode());
            }
        }
        #endregion

        #region accumulator buffer
        internal void trackEntity(Entity e)
        {
            var score = calcStateScore(e);
            int stored_score;
            if (_accumBuffer.TryGetValue(e, out stored_score))
            {
                score += stored_score;
            }
            _accumBuffer[e] = score;
        }

        internal void UntrackEntity(Entity e)
        {
            if (_accumBuffer.ContainsKey(e))
                _accumBuffer.Remove(e);
        }

        internal List<Entity> GetTrackedEntitiesByScore(int maxCount=5)
        {
            var list= _accumBuffer.OrderByDescending(kvp => kvp.Value)
                .Take(maxCount).Select<KeyValuePair<Entity, int>, Entity>(kvp => kvp.Key)
                .ToList();

            foreach (var e in list)
                _accumBuffer.Remove(e);

            return list;
        }

        internal int calcStateScore(Entity e)
        {
            var playerComp = e.GetComponent<PlayerComponent>();
            if (playerComp == this)
                return 100000; //high pri ourselves to always sync.

            var phyComp = e.GetComponent<Physics2dComponent>();
            if (phyComp != null && phyComp.Body != null)
            {
                var lvel = phyComp.Body.LinearVelocity;
                if (lvel.LengthSquared() >= 0.0001f)
                    return 10; // moving objects get a pri boost
            }

            return 1; // default to a very small pri boost to pop above dead objects.
        }
        #endregion
    }
}
