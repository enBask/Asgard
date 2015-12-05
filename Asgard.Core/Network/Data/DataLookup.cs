using Artemis;
using Asgard.Core.System;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        private PropertyInfo _property;
        private PropertyInfo PropGetter;
        private Type _resolvedType;
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
            _property = prop;
            var type = typeof(NetworkProperty<>);
            _resolvedType = type.MakeGenericType(propType);
            PropGetter = _resolvedType.GetProperty("Value");

            //NumberOfBits = BitsToHoldValue(Attribute._range);

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

        public object Get(object owner)
        {
            if (_property == null) return null;
            if (owner == null) return null;

            var propObject = _property.GetValue(owner);
            if (propObject == null)
            {
                propObject = Activator.CreateInstance(_resolvedType);
                _property.SetValue(owner, propObject);
            }
            return PropGetter.GetValue(propObject);
        }

        public void Set(object owner, object value)
        {
            if (_property == null) return;
            if (owner == null) return;

            var propObject = _property.GetValue(owner);
            if (propObject == null)
            {
                propObject = Activator.CreateInstance(_resolvedType);
                _property.SetValue(owner, propObject);
            }
            PropGetter.SetValue(propObject, value);
        }

        public object CreateChildProperty()
        {
            if (_childProperty == null)
                return null;

            return Activator.CreateInstance(_childProperty.ResolvedType);

        }

        internal void DefineObject(object owner, Entity entity)
        {
            var defObj = Get(owner);

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
                prop.Set(objA, prop.Get(objB));
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
