using Artemis;
using Asgard.Core.System;
using Lidgren.Network;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FastMember;
using Artemis.Interface;

namespace Asgard.Core.Network.Data
{
    internal enum DataTypes
    {
        BOOL = 0,
        UBYTE,
        BYTE,
        USHORT,
        SHORT,
        UINT,
        INT,
        ULONG,
        LONG,
        FLOAT,
        DOUBLE,
        STRING,
        VECTOR2,
        NETPROP
    }

    internal class DataSerializationProperty
    {
        internal string propName;
        private Type resolvedType;
        private DataSerializationItem _childProperty;

        public DataTypes Type { get; set; }

        internal static int BitsToHoldValue(ulong value)
        {
            int bits = 1;
            while ((value >>= 1) != 0)
                bits++;
            return bits;
        }

        internal DataSerializationItem ChildProperty { get{ return _childProperty; } }

        public DataSerializationProperty(PropertyInfo prop, Type propType)
        {
            if (prop != null)
            {
                propName = prop.Name;
            }

            resolvedType = propType;
            #region type conv
            if (propType == typeof(bool))
            {
                Type = DataTypes.BOOL;
            }
            else if (propType == typeof(byte))
            {
                Type = DataTypes.UBYTE;
            }
            else if (propType == typeof(sbyte))
            {
                Type = DataTypes.BYTE;

            }
            else if (propType == typeof(ushort))
            {
                Type = DataTypes.USHORT;
            }
            else if (propType == typeof(short))
            {
                Type = DataTypes.SHORT;
            }
            else if (propType == typeof(uint))
            {
                Type = DataTypes.UINT;
            }
            else if (propType == typeof(int))
            {
                Type = DataTypes.INT;
            }
            else if (propType == typeof(ulong))
            {
                Type = DataTypes.ULONG;
            }
            else if (propType == typeof(long))
            {
                Type = DataTypes.LONG;
            }
            else if (propType == typeof(float))
            {
                Type = DataTypes.FLOAT;
            }
            else if (propType == typeof(double))
            {
                Type = DataTypes.DOUBLE;
            }
            else if (propType == typeof(string))
            {
                Type = DataTypes.STRING;
            }
            else if (propType == typeof(Vector2))
            {
                Type = DataTypes.VECTOR2;
            }

            if (propType.IsSubclassOf(typeof(NetworkObject)))
            {
                Type = DataTypes.NETPROP;
                var typeInfo = propType.GetTypeInfo();
                _childProperty = DataLookupTable.BuildDataItem(typeInfo);
            }

            #endregion
        }

        public T Get<T>(NetworkObject owner)
        {
            if (owner == null) return default(T);

            var wrapped = ObjectAccessor.Create(owner);
            var prop = wrapped[propName];

            if (_childProperty != null)
            {
                var t = typeof(NetworkProperty<>);
                t = t.MakeGenericType(_childProperty.ResolvedType);
                if (prop == null)
                {
                    prop = Activator.CreateInstance(t);
                    wrapped[propName] = prop;
                }

                var subWrap = ObjectAccessor.Create(prop);
                return (T)subWrap["Value"];
            }
            else
            {
                if (prop != null)
                {
                    return (prop as NetworkProperty<T>).Value;
                }
                return default(T);
            }


        }

        public object GetUnknown(NetworkObject owner)
        {
            if (owner == null) return null;

            var wrapped = ObjectAccessor.Create(owner);
            var prop = wrapped[propName];

            var t = typeof(NetworkProperty<>);
            t = t.MakeGenericType(resolvedType);
            if (prop == null)
            {
                return null;
            }

            var subWrap = ObjectAccessor.Create(prop);
            return subWrap["Value"];
        }

        public void Set<T>(NetworkObject owner, T value)
        {
            if (owner == null) return;
            var wrapped = ObjectAccessor.Create(owner);
            var prop = wrapped[propName];

            if (_childProperty != null)
            {
                var t = typeof(NetworkProperty<>);
                t = t.MakeGenericType(_childProperty.ResolvedType);
                if (prop == null)
                {
                    prop = Activator.CreateInstance(t);
                    wrapped[propName] = prop;
                }

                var subWrap = ObjectAccessor.Create(prop);
                subWrap["Value"] = value;
            }
            else
            {
                if (prop == null)
                {
                    prop = new NetworkProperty<T>();
                    (prop as NetworkProperty<T>).Value = value;
                    wrapped[propName] = prop;
                }
                else
                {
                    (prop as NetworkProperty<T>).Value = value;
                }
                
            }

            
        }

        public void NetworkClone(NetworkObject owner, object value)
        {
            if (owner == null) return;
            var wrapped = ObjectAccessor.Create(owner);
            var prop = wrapped[propName];

            var valWrap = ObjectAccessor.Create(value);
            var valProp = valWrap[propName];
            if (valProp == null) return;


            var t = typeof(NetworkProperty<>);
            t = t.MakeGenericType(resolvedType);
            if (prop == null)
            {
                prop = Activator.CreateInstance(t);
                wrapped[propName] = prop;
            }

            var subWrap = ObjectAccessor.Create(prop);
            var subValWrap = ObjectAccessor.Create(valProp);

            if (_childProperty != null)
            {
                NetworkObject netObj = subValWrap["Value"] as NetworkObject;
                subWrap["Value"] = netObj.NetworkClone();
            }
            else
            {
                subWrap["Value"] = subValWrap["Value"];
            }
        }

        internal bool CheckBaseline<T>(NetworkObject owner, NetworkObject _baseLine)
            where T : struct
        {
            var a = Get<T>(owner);
            var b = Get<T>(_baseLine);
            return a.Equals(b);
        }

        public void SetUnknown(NetworkObject owner, object value)
        {
            if (owner == null) return;
            if (value == null) return;
            var wrapped = ObjectAccessor.Create(owner);
            var prop = wrapped[propName];

            var t = typeof(NetworkProperty<>);
            t = t.MakeGenericType(resolvedType);
            if (prop == null)
            {
                prop = Activator.CreateInstance(t);
                var subWrap = ObjectAccessor.Create(prop);
                subWrap["Value"] = value;
                wrapped[propName] = prop;
            }
            else
            {
                var subWrap = ObjectAccessor.Create(prop);
                subWrap["Value"] = value;

            }

        }

        internal void UnDefineObject(NetworkObject owner, Entity entity)
        {
            var defObj = Get<NetworkObject>(owner);

            var objType = defObj.GetType();
            var ditem = DataLookupTable.Get(objType.GetTypeInfo());
            foreach (var prop in ditem.Properties)
            {
                if (prop.ChildProperty != null)
                {
                    prop.UnDefineObject(defObj, entity);
                }
            }
            ObjectMapper.UnDefineObject(defObj, (uint)entity.UniqueId);
        }

        public object CreateChildProperty()
        {
            if (_childProperty == null)
                return null;

            return Activator.CreateInstance(_childProperty.ResolvedType);

        }

        internal void DefineObject(NetworkObject owner, Entity entity)
        {
            var defObj = Get<NetworkObject>(owner);

            var objType = defObj.GetType();
            var ditem = DataLookupTable.Get(objType.GetTypeInfo());
            foreach (var prop in ditem.Properties)
            {
                if (prop.ChildProperty != null)
                {
                    prop.DefineObject(defObj, entity);
                }
            }
            ObjectMapper.DefineObject(defObj, (uint)entity.UniqueId);
        }

    }

    internal class DataSerializationItem
    {
        public bool IsUnreliable { get; set; }
        public List<DataSerializationProperty> Properties { get; set; }

        public DataSerializationItem(Type resolvedType)
        {
            ResolvedType = resolvedType;
            Properties = new List<DataSerializationProperty>();
        }

        public Type ResolvedType { get; private set; }

        public void Merge(NetworkObject objA, NetworkObject objB)
        {
            foreach(var prop in Properties)
            {
                prop.SetUnknown(objA, prop.GetUnknown(objB));
            }

            objA.IsUpdated = true;
        }
    }

    internal static class DataLookupTable
    {
        private static Dictionary<TypeInfo, DataSerializationItem> _LookupTable =
            new Dictionary<TypeInfo, DataSerializationItem>();

        internal static bool CheckType(TypeInfo type)
        {
            if (type.IsSubclassOf(typeof(NetworkObject)))
            {
                return true;
            }

            return false;
        }

        internal static void AddType(TypeInfo type)
        {
            if (type.IsSubclassOf(typeof(NetworkObject)))
            {
                AddTypeObject(type);
            }
        }

        internal static DataSerializationItem BuildDataItem(TypeInfo type)
        {
            if (!type.IsSubclassOf(typeof(NetworkObject)))
                return null;

            var dataClass = new DataSerializationItem(type);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var propType = prop.PropertyType;
                if (propType.IsGenericType)
                {
                    var genType = propType.GetGenericTypeDefinition();
                    if (genType == typeof(NetworkProperty<>))
                    {
                        var types = propType.GetGenericArguments();
                        if (types == null || types.Length != 1)
                            continue;

                        var typeArg = types[0];
                        DataSerializationProperty dataProp = new DataSerializationProperty(prop, typeArg);
                        dataClass.Properties.Add(dataProp);
                    }
                }
            }

            return dataClass;
        }      
        internal static void AddTypeObject(TypeInfo type)
        {
            ObjectMapper.AddRawType(type);

            bool isUnreliable = type.IsSubclassOf(typeof(UnreliableStateSyncNetworkObject));

            DataSerializationItem dataItem = BuildDataItem(type);
            dataItem.IsUnreliable = isUnreliable;
            _LookupTable[type] = dataItem;
        }

        internal static DataSerializationItem Get(TypeInfo type)
        {
            DataSerializationItem dItem;
            _LookupTable.TryGetValue(type, out dItem);
            return dItem;
        }
    }
}
