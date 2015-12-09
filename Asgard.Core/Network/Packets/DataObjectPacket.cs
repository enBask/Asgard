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
        public uint SnapId { get; set; }
        public uint BaselineId { get; set; }

        DataSerializationItem _dItem;
        NetworkObject _owner;
        NetworkObject _baseLine;
        TypeInfo _ownerType;

        public DataObjectPacket()
        {
            SnapId = NetTime.SimTick;
        }

        public void SetOwnerObject(NetworkObject obj)
        {
            _owner = obj;
            _ownerType = obj.GetType().GetTypeInfo();
            _dItem = DataLookupTable.Get(_ownerType);
            if (_dItem.IsUnreliable)
            {
                Method = NetDeliveryMethod.UnreliableSequenced;
            }
        }

        public void SetBaseline(NetworkObject obj)
        {
            _baseLine = obj;
        }

        public override void Deserialize(Bitstream msg)
        {
            ushort objTypeId = msg.ReadUInt16();
            Id = msg.ReadInt64();
            SnapId = msg.ReadUInt32();
            BaselineId = msg.ReadUInt32();


            object obj = ObjectMapper.Lookup(Id, objTypeId);
            if (obj == null)
            {
                obj = ObjectMapper.Create(Id, objTypeId);
            }
            SetOwnerObject(obj as NetworkObject);

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
            msg.Write(objTypeId);
            msg.Write(Id);
            msg.Write(SnapId);
            msg.Write(BaselineId);
            WriteNetObject(_dItem, msg, _owner);
        }

        private bool UseBaseline(DataSerializationProperty prop, NetworkObject owner)
        {
            return false;
            if (_baseLine == null) return false;
            switch (prop.Type)
            {
                case DataTypes.BOOL:
                {
                    bool a = prop.Get<bool>(owner);
                    bool b = prop.Get<bool>(_baseLine);
                    return a == b;
                }
                case DataTypes.BYTE:
                {
                    sbyte a = prop.Get<sbyte>(owner);
                    sbyte b = prop.Get<sbyte>(_baseLine);
                    return a == b;
                }
                case DataTypes.DOUBLE:
                {
                    double a = prop.Get<double>(owner);
                    double b = prop.Get<double>(_baseLine);
                    return a == b;
                }
                case DataTypes.FLOAT:
                {
                    float a = prop.Get<float>(owner);
                    float b = prop.Get<float>(_baseLine);
                    return a == b;
                }
                case DataTypes.INT:
                {
                    int a = prop.Get<int>(owner);
                    int b = prop.Get<int>(_baseLine);
                    return a == b;
                }
                case DataTypes.LONG:
                {
                    long a = prop.Get<long>(owner);
                    long b = prop.Get<long>(_baseLine);
                    return a == b;
                }
                case DataTypes.SHORT:
                {
                    short a = prop.Get<short>(owner);
                    short b = prop.Get<short>(_baseLine);
                    return a == b;
                }
                case DataTypes.UBYTE:
                {
                    byte a = prop.Get<byte>(owner);
                    byte b = prop.Get<byte>(_baseLine);
                    return a == b;
                }
                case DataTypes.UINT:
                {
                    uint a = prop.Get<uint>(owner);
                    uint b = prop.Get<uint>(_baseLine);
                    return a == b;
                }
                case DataTypes.ULONG:
                {
                    ulong a = prop.Get<ulong>(owner);
                    ulong b = prop.Get<ulong>(_baseLine);
                    return a == b;
                }
                case DataTypes.USHORT:
                {
                    ushort a = prop.Get<ushort>(owner);
                    ushort b = prop.Get<ushort>(_baseLine);
                    return a == b;
                }
                case DataTypes.VECTOR2:
                {
                    Vector2 a = prop.Get<Vector2>(owner);
                    Vector2 b = prop.Get<Vector2>(_baseLine);
                    return a == b;
                }
                case DataTypes.NETPROP:
                {
                    var childProp = prop.ChildProperty;
                    if (childProp != null)
                    {
                        var o = prop.Get<NetworkObject>(owner);

                        var baseline = _baseLine;
                        var childBase = prop.Get<NetworkObject>(_baseLine);

                        _baseLine = childBase;    
                        bool bUse = true;                    
                        foreach(var child_prop in childProp.Properties)
                        {
                            if (!UseBaseline(child_prop, o))
                            {
                                bUse = false;
                                break;
                            }
                        }
                        _baseLine = baseline;
                        return bUse;
                    }

                    return true;
                }
            }

            return false;
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
                var useBaseline = msg.ReadBool();
                if (useBaseline)
                {
                    continue;
                }

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
                if (UseBaseline(prop, owner))
                {
                    msg.Write(true);
                    continue;
                }
                else
                {
                    msg.Write(false);
                }

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

                            var baseline = _baseLine;
                            if (_baseLine != null)
                            {
                                var childBase = prop.Get<NetworkObject>(_baseLine);
                                _baseLine = childBase;
                            }
                            WriteNetObject(childProp, msg, o);
                            _baseLine = baseline;
                        }
                        break;
                }
            }
        }
    }
}
