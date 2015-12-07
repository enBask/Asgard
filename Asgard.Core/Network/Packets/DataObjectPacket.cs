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
            ushort objTypeId = msg.ReadUInt16();
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

            ReadNetObject(_dItem, msg, (NetworkObject)_owner);
        }

        public override void Serialize(Bitstream msg)
        {
            if (_dItem == null)
            {
                return;
            }

            ushort objTypeId = ObjectMapper.LookupType(_ownerType);
            msg.Write(objTypeId);
            msg.Write(Id);
            WriteNetObject(_dItem, msg, (NetworkObject)_owner);
        }

        private void ReadNetObject(DataSerializationItem item, Bitstream msg, NetworkObject owner)
        {
            if (owner is DefinitionNetworkObject)
            {
                var d = msg.ReadBool();
                if (d)
                {
                    (owner as DefinitionNetworkObject).Destory = true;
                    return;
                }
            }

            foreach (var prop in item.Properties)
            {
                switch (prop.Type)
                {
                    case DataTypes.BOOL:
                        prop.Set(owner, msg.ReadBool());
                        break;
                    case DataTypes.UBYTE:
                        prop.Set(owner, msg.ReadByte());
                        break;
                    case DataTypes.UINT:
                        prop.Set(owner, msg.ReadUInt32());
                        break;
                    case DataTypes.BYTE:
                        prop.Set(owner, msg.ReadSByte());
                        break;
                    case DataTypes.INT:
                        prop.Set(owner, msg.ReadInt32());
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
                            var o = (NetworkObject)prop.CreateChildProperty();
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

        private void WriteNetObject(DataSerializationItem item, Bitstream msg, NetworkObject owner)
        {
            if (owner is DefinitionNetworkObject)
            {
                if ((owner as DefinitionNetworkObject).Destory)
                {
                    msg.Write(true);
                    return;
                }
                else
                {
                    msg.Write(false);
                }
            }

            foreach (var prop in item.Properties)
            {
                switch (prop.Type)
                {
                    case DataTypes.BOOL:
                        msg.Write(prop.Get<bool>(owner));
                        break;
                    case DataTypes.UBYTE:
                        msg.Write(prop.Get<byte>(owner));
                        break;
                    case DataTypes.UINT:
                        msg.Write(prop.Get<uint>(owner));
                        break;
                    case DataTypes.BYTE:
                        msg.Write(prop.Get<sbyte>(owner));
                        break;
                    case DataTypes.INT:
                        msg.Write(prop.Get<int>(owner));
                        break;
                    case DataTypes.ULONG:
                        msg.Write(prop.Get<ulong>(owner));
                        break;
                    case DataTypes.LONG:
                        msg.Write(prop.Get<long>(owner));
                        break;
                    case DataTypes.FLOAT:
                        msg.Write(prop.Get<float>(owner));
                        break;
                    case DataTypes.DOUBLE:
                        msg.Write(prop.Get<double>(owner));
                        break;
                    case DataTypes.STRING:
                        msg.Write(prop.Get<string>(owner));
                        break;
                    case DataTypes.VECTOR2:
                        msg.Write(prop.Get<Vector2>(owner));
                        break;
                    case DataTypes.NETPROP:
                        var childProp = prop.ChildProperty;
                        if (childProp != null)
                        {
                            var o = prop.Get<NetworkObject>(owner);
                            WriteNetObject(childProp, msg, o);
                        }
                        break;
                }
            }
        }
    }
}
