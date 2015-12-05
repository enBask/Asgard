using Asgard.Core.Network.Data;
using Asgard.Core.System;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    [PacketAttribute((ushort)PacketTypes.DATA_OBJECT, NetDeliveryMethod.ReliableOrdered)]
    public class DataObjectPacket : Packet
    {        
        public long Id { get; set; }

        DataSerializationItem _dItem;
        object _owner;
        TypeInfo _ownerType;

        public DataObjectPacket()
        {

        }

        public void SetOwnerObject(object obj)
        {
            _owner = obj;
            _ownerType = obj.GetType().GetTypeInfo();
            _dItem = DataLookupTable.Get(_ownerType);
            if (_dItem.IsUnreliable)
            {
                Method = NetDeliveryMethod.UnreliableSequenced;
            }
        }

        public override void Deserialize(Bitstream msg)
        {
            ushort objTypeId = (ushort)msg.ReadVariableUInt32();
            Id = msg.ReadInt64();


            object obj = ObjectMapper.Lookup(Id, objTypeId);
            if (obj == null)
            {
                obj = ObjectMapper.Create(Id, objTypeId);
            }
            SetOwnerObject(obj);

            if (_dItem == null)
            {
                return;
            }

            ReadNetObject(_dItem, msg, _owner);
        }

        public override void Serialize(Bitstream msg)
        {
            if (_dItem == null)
            {
                return;
            }

            ushort objTypeId = ObjectMapper.LookupType(_ownerType);
            msg.WriteVariableUInt32(objTypeId);
            msg.Write(Id);
            WriteNetObject(_dItem, msg, _owner);
        }

        private void ReadNetObject(DataSerializationItem item, Bitstream msg, object owner)
        {
            foreach (var prop in item.Properties)
            {
                switch (prop.Type)
                {
                    case DataTypes.BOOL:
                        prop.Set(owner, msg.ReadBool());
                        break;
                    case DataTypes.UBYTE:
                    case DataTypes.UINT:
                        prop.Set(owner, msg.ReadVariableUInt32());
                        break;
                    case DataTypes.BYTE:
                    case DataTypes.INT:
                        prop.Set(owner, msg.ReadVariableInt32());
                        break;
                    case DataTypes.ULONG:
                        prop.Set(owner, msg.ReadUInt64());
                        break;
                    case DataTypes.LONG:
                        prop.Set(owner, msg.ReadInt64());
                        break;
                    case DataTypes.FLOAT:
                        prop.Set(owner, msg.ReadFloat());
                        break;
                    case DataTypes.DOUBLE:
                        prop.Set(owner, msg.ReadDouble());
                        break;
                    case DataTypes.STRING:
                        prop.Set(owner, msg.ReadString());
                        break;
                    case DataTypes.VECTOR2:
                        prop.Set(owner, msg.ReadVector2());
                        break;
                    case DataTypes.NETPROP:
                        var childProp = prop.ChildProperty;
                        if (childProp != null)
                        {
                            var o = prop.CreateChildProperty();
                            if (o != null)
                            {
                                ReadNetObject(childProp, msg, o);
                                prop.Set(owner, o);
                            }
                        }
                        break;
                }
            }
        }

        private void WriteNetObject(DataSerializationItem item, Bitstream msg, object owner)
        {
            foreach (var prop in item.Properties)
            {
                switch (prop.Type)
                {
                    case DataTypes.BOOL:
                        msg.Write((bool)prop.Get(owner));
                        break;
                    case DataTypes.UBYTE:
                    case DataTypes.UINT:
                        msg.WriteVariableUInt32((uint)prop.Get(owner));
                        break;
                    case DataTypes.BYTE:
                    case DataTypes.INT:
                        msg.WriteVariableInt32((int)prop.Get(owner));
                        break;
                    case DataTypes.ULONG:
                        msg.Write((ulong)prop.Get(owner));
                        break;
                    case DataTypes.LONG:
                        msg.Write((long)prop.Get(owner));
                        break;
                    case DataTypes.FLOAT:
                        msg.Write((float)prop.Get(owner));
                        break;
                    case DataTypes.DOUBLE:
                        msg.Write((double)prop.Get(owner));
                        break;
                    case DataTypes.STRING:
                        msg.Write((string)prop.Get(owner));
                        break;
                    case DataTypes.VECTOR2:
                        msg.Write((Vector2)prop.Get(owner));
                        break;
                    case DataTypes.NETPROP:
                        var childProp = prop.ChildProperty;
                        if (childProp != null)
                        {
                            var o = prop.Get(owner);
                            WriteNetObject(childProp, msg, o);
                        }
                        break;
                }
            }
        }
    }
}
