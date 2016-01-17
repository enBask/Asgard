using Artemis;
using Artemis.Interface;
using Artemis.Manager;
using Asgard.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.System
{
    public static class ObjectMapper
    {
        static EntityManager _manager;
        static AsgardBase _instance;

        static Dictionary<ushort, Type> _typeLookup = new Dictionary<ushort, Type>();
        static Dictionary<Type, ushort> _typeLookupReverse = new Dictionary<Type, ushort>();
        static List<TypeInfo> _rawLookupTypes = new List<TypeInfo>();

        static Dictionary<Entity, List<NetworkObject>> _netObjectCache =
            new Dictionary<Entity, List<NetworkObject>>();

        static List<Tuple<Entity, NetworkObject>> _snapshotCache = new List<Tuple<Entity, NetworkObject>>();
        static bool _inSnapshot = false;

        static Dictionary<int, Collections.LinkedList<Tuple<uint, NetworkObject>>> _deltaStates =
            new Dictionary<int, Collections.LinkedList<Tuple<uint, NetworkObject>>>();

        internal static void Init(AsgardBase instance)
        {
            _instance = instance;
            _manager = instance.EntityManager;

            var orderedList = _rawLookupTypes.OrderBy(t =>
            {
                return t.ToString().GetHashCode();
            });

            ushort index = 0;
            foreach(var item in orderedList)
            {
                _typeLookup[index] = item;
                _typeLookupReverse[item] = index;
                index++;
            }
        }

        internal static void AddRawType(TypeInfo type)
        {
            _rawLookupTypes.Add(type);
        }

        internal static Type LookupType(ushort id)
        {
            Type type;
            _typeLookup.TryGetValue(id, out type);
            return type;
        }

        internal static ushort LookupType(Type type)
        {
            ushort id;
            _typeLookupReverse.TryGetValue(type, out id);
            return id;
        }

        public static void DefineObject(object obj, long entityId)
        {
            if (obj is DefinitionNetworkObject)
            {
                var entity = _manager.GetEntityByUniqueId(entityId);
                (obj as DefinitionNetworkObject).OnCreated(_instance, entity);
            }
        }
        internal static void UnDefineObject(object obj, long entityId)
        {
            if (obj is DefinitionNetworkObject)
            {
                var entity = _manager.GetEntityByUniqueId(entityId);
                (obj as DefinitionNetworkObject).OnDestroyed(_instance, entity);
            }
        }

        public static NetworkObject Lookup(long id, ushort typeId, bool snapshot=true)
        {
            var entity = _manager.GetEntityByUniqueId(id);
            if (entity == null) return null;


            var type = LookupType(typeId);
            var compType = ComponentTypeManager.GetTypeFor(type);
            var comp = entity.GetComponent(compType) as NetworkObject;

            if (snapshot)
            {
                List<NetworkObject> objList;
                if (_netObjectCache.TryGetValue(entity, out objList))
                {
                    objList.Add(comp);
                }
                else
                {
                    objList = new List<NetworkObject>();
                    objList.Add(comp);
                    _netObjectCache[entity] = objList;
                }

                _snapshotCache.Add(new Tuple<Entity, NetworkObject>(entity, comp));
            }

            return comp;
        }

        public static NetworkObject Create(uint id, ushort typeId)
        {
            var compType = LookupType(typeId);
            return Create(id, compType);            
        }

        public static NetworkObject Create(uint id, Type type)
        {
            var entity = CreateEntity(id);

            var comp = Activator.CreateInstance(type) as NetworkObject;

            List<NetworkObject> objList;
            if (_netObjectCache.TryGetValue(entity, out objList))
            {
                objList.Add(comp);
            }
            else
            {
                objList = new List<NetworkObject>();
                objList.Add(comp);
                _netObjectCache[entity] = objList;
            }

            if (_inSnapshot && comp != null)
            {
                _snapshotCache.Add(new Tuple<Entity, NetworkObject>(entity, comp));
            }
            else
            {
                var compType = ComponentTypeManager.GetTypeFor(type);
                if (entity.GetComponent(compType) == null)
                {
                    entity.AddComponent(comp as IComponent);
                }

            }

            return comp;
        }

        internal static List<NetworkObject> GetNetObjects(Entity entity, Type type)
        {
            List<NetworkObject> objList;
            _netObjectCache.TryGetValue(entity, out objList);

            if (objList == null) return null;

            return objList.Where(o => o.GetType().IsSubclassOf(type)).ToList();
        }

        internal static IEnumerable<Entity> GetEntityCache()
        {
            return _netObjectCache.Keys.ToList();
        }

        public static void DestroyNetObject(Entity ent, NetworkObject netObj)
        {
            List<NetworkObject> objList;
            if (_netObjectCache.TryGetValue(ent, out objList))
            {
                objList.Remove(netObj);
            }
        }

        public static Entity CreateEntity(uint id = 0)
        {
            Entity ent = _manager.GetEntityByUniqueId(id);
            if (ent == null)
            {
                ent = CreateEntityById(id);
                _netObjectCache[ent] = new List<NetworkObject>();
            }

            return ent;
        }

        static object _lock_obj = new object();
        static uint _master_entity_id = 0;
        public static Entity CreateEntityById(uint id=0)
        {
            if (id == 0)
            {
                id = ++_master_entity_id;
                var e = _manager.Create(id);
                return e;
            }
            else
            {
                var e = _manager.GetEntityByUniqueId(id);
                if (e == null)
                {
                    e = _manager.Create(id);
                }
                return e;
            }
        }

        public static void DestroyEntity(Entity ent, bool destoryEntity=true)
        {
            List<NetworkObject> objCache;
            if (_netObjectCache.TryGetValue(ent, out objCache))
            {
                _netObjectCache.Remove(ent);

                foreach(var obj in objCache)
                {
                    if (obj is DefinitionNetworkObject)
                    {
                        (obj as DefinitionNetworkObject).Destroy = true;
                        (obj as DefinitionNetworkObject).OnDestroyed(_instance, ent, false);
                    }
                }
            }

            if (destoryEntity)
            {
                _manager.Remove(ent);
            }
        }
       

        public static void StartSnapshot()
        {
            _snapshotCache.Clear();
            _inSnapshot = true;
        }

        public static List<Tuple<Entity, NetworkObject>> EndSnapshot()
        {
            var tmpList = new List<Tuple<Entity, NetworkObject>>(_snapshotCache);
            _snapshotCache.Clear();
            _inSnapshot = false;
            return tmpList;
        }

        internal static void SetCurrentPlayerState(uint id, PlayerStateData data)
        {
            var e = _manager.GetEntityByUniqueId(id);
            EntitySystems.Components.PlayerComponent pc =
                e.GetComponent<EntitySystems.Components.PlayerComponent>();
            if (pc != null)
            {
                pc.LastSyncState = data;
            }

        }
        internal static PlayerStateData GetCurrentPlayerState(uint id)
        {
            var e = _manager.GetEntityByUniqueId(id);
            EntitySystems.Components.PlayerComponent pc = 
                e.GetComponent<EntitySystems.Components.PlayerComponent>();
            if (pc != null)
            {
                return pc.LastSyncState;
            }

            return null;
        }

        internal static void AddDeltaState(int objHash, uint tickId, NetworkObject obj)
        {

            if (objHash == 0) return;

            Collections.LinkedList<Tuple<uint, NetworkObject>> objList;
            if (!_deltaStates.TryGetValue(objHash, out objList))
            {
                _deltaStates[objHash] = objList = new
                Collections.LinkedList<Tuple<uint, NetworkObject>>();
            }

            objList.AddToTail(new Tuple<uint, NetworkObject>(tickId, obj));
        }

        internal static NetworkObject GetBaseline(uint baselineId, int objHash)
        {
            Collections.LinkedList<Tuple<uint, NetworkObject>> objList;
            _deltaStates.TryGetValue(objHash, out objList);

            if (objList != null)
            {
                foreach(var node in objList)
                {
                    if (node.Value.Item1 == baselineId)
                    {
                        objList.TruncateTo(node);
                        return node.Value.Item2;
                    }
                }
            }

            return null;
        }

        internal static uint LastSimId = 0;
        internal static uint GetLastSimId()
        {
            return LastSimId;
        }

    }
}
