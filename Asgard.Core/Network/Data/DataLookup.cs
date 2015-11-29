using Asgard.Core.System;
using Lidgren.Network;
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
        STRING
    }

    internal class DataSerializationProperty
    {
        private PropertyInfo _property;
        private PropertyInfo PropGetter;
        private Type _resolvedType;

        public DataTypes Type { get; set; }

        internal static int BitsToHoldValue(ulong value)
        {
            int bits = 1;
            while ((value >>= 1) != 0)
                bits++;
            return bits;
        }

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


    }

    internal class DataSerializationItem
    {
        public bool IsUnreliable { get; set; }
        public List<DataSerializationProperty> Properties { get; set; }

        public DataSerializationItem()
        {
            Properties = new List<DataSerializationProperty>();
        }

        public void Merge(object objA, object objB)
        {
            foreach(var prop in Properties)
            {
                prop.Set(objA, prop.Get(objB));
            }
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
            ObjectMapper.AddRawType(type);

            bool isUnreliable = (type.AsType() == typeof(UnreliableNetworkObject));

            DataSerializationItem dataItem = new DataSerializationItem();
            dataItem.IsUnreliable = isUnreliable;

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach(var prop in props)
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
                        dataItem.Properties.Add(dataProp);

                    }
                }
            }

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
