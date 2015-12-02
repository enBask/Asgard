using Artemis;
using Artemis.Interface;
using Artemis.Manager;
using Asgard.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.System
{
    public static class ObjectMapper
    {
        static EntityManager _manager;

        static Dictionary<ushort, Type> _typeLookup = new Dictionary<ushort, Type>();
        static Dictionary<Type, ushort> _typeLookupReverse = new Dictionary<Type, ushort>();
        static List<TypeInfo> _rawLookupTypes = new List<TypeInfo>();

        static Dictionary<Entity, List<NetworkObject>> _netObjectCache =
            new Dictionary<Entity, List<NetworkObject>>();

        static List<Tuple<Entity, NetworkObject>> _snapshotCache = new List<Tuple<Entity, NetworkObject>>();
        static bool _inSnapshot = false;

        internal static void Init(EntityManager manager)
        {
            _manager = manager;

            var orderedList = _rawLookupTypes.OrderBy(t =>
            {
                return t.AssemblyQualifiedName.GetHashCode();
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

        public static NetworkObject Lookup(uint id, ushort typeId)
        {
            var entity = _manager.GetEntityByUniqueId(id);
            if (entity == null) return null;


            var type = LookupType(typeId);
            var compType = ComponentTypeManager.GetTypeFor(type);
            var comp = entity.GetComponent(compType) as NetworkObject;

            if (_inSnapshot && comp != null)
            {
                comp = Activator.CreateInstance(type) as NetworkObject;
                _snapshotCache.Add( new Tuple<Entity, NetworkObject>(entity, comp));
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
                comp = Activator.CreateInstance(type) as NetworkObject;
                _snapshotCache.Add(new Tuple<Entity, NetworkObject>(entity, comp));
            }
            else
            {
                entity.AddComponent(comp as IComponent);
            }

            return comp;
        }

        internal static List<NetworkObject> GetNetObjects(Entity entity)
        {
            List<NetworkObject> objList;
            _netObjectCache.TryGetValue(entity, out objList);
            return objList;
        }

        internal static IEnumerable<Entity> GetEntityCache()
        {
            foreach( var e in _netObjectCache.Keys)
            {
                yield return e;
            }
        }

        public static void DestroyNetObject(Entity ent, NetworkObject netObj)
        {
            List<NetworkObject> objList;
            if (_netObjectCache.TryGetValue(ent, out objList))
            {
                objList.Remove(netObj);
            }
        }

        public static Entity CreateEntity(uint id)
        {
            var ent = _manager.GetEntityByUniqueId(id);
            if (ent == null)
            {
                ent = _manager.Create(id);
                _netObjectCache[ent] = new List<NetworkObject>();
            }

            return ent;
        }

        public static void DestoryEntity(Entity ent)
        {
            _manager.Remove(ent);
            _netObjectCache.Remove(ent);
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
    }
}
