using Asgard.Core.Network.Data;
using Asgard.Core.System;
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
        public uint Id { get; set; }

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
            Id = msg.ReadVariableUInt32();


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


            foreach (var prop in _dItem.Properties)
            {
                switch (prop.Type)
                {
                    case DataTypes.BOOL:
                        prop.Set(_owner, msg.ReadBool());
                        break;
                    case DataTypes.UBYTE:
                    case DataTypes.UINT:
                        prop.Set(_owner, msg.ReadVariableUInt32() );
                        break;
                    case DataTypes.BYTE:
                    case DataTypes.INT:
                        prop.Set(_owner, msg.ReadVariableInt32());
                        break;
                    case DataTypes.ULONG:
                        prop.Set(_owner, msg.ReadUInt64());
                        break;
                    case DataTypes.LONG:
                        prop.Set(_owner, msg.ReadInt64());
                        break;
                    case DataTypes.FLOAT:
                        prop.Set(_owner, msg.ReadFloat());
                        break;
                    case DataTypes.DOUBLE:
                        prop.Set(_owner, msg.ReadDouble());
                        break;
                    case DataTypes.STRING:
                        prop.Set(_owner, msg.ReadString());
                        break;
                }
            }

        }

        public override void Serialize(Bitstream msg)
        {
            if (_dItem == null)
            {
                return;
            }

            ushort objTypeId = ObjectMapper.LookupType(_ownerType);
            msg.WriteVariableUInt32(objTypeId);
            msg.WriteVariableUInt32(Id);
            foreach(var prop in _dItem.Properties)
            {
                switch(prop.Type)
                {
                    case DataTypes.BOOL:
                        msg.Write((bool)prop.Get(_owner));
                        break;
                    case DataTypes.UBYTE:
                    case DataTypes.UINT:
                        msg.WriteVariableUInt32((uint)prop.Get(_owner));
                        break;
                    case DataTypes.BYTE:
                    case DataTypes.INT:
                        msg.WriteVariableInt32((int)prop.Get(_owner));
                        break;
                    case DataTypes.ULONG:
                        msg.Write((ulong)prop.Get(_owner));
                        break;
                    case DataTypes.LONG:
                        msg.Write((long)prop.Get(_owner));
                        break;
                    case DataTypes.FLOAT:
                        msg.Write((float)prop.Get(_owner));
                        break;
                    case DataTypes.DOUBLE:
                        msg.Write((double)prop.Get(_owner));
                        break;
                    case DataTypes.STRING:
                        msg.Write((string)prop.Get(_owner));
                        break;
                }
            }
        }
    }
}
