using Asgard.Core.Network.Data;
using Asgard.Core.System;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    [PacketAttribute((ushort)PacketTypes.DATA_OBJECT, NetDeliveryMethod.ReliableOrdered)]
    public class DataObjectPacket : Packet
    {
        public uint SnapId { get; set; }
        public uint BaselineId { get; set; }

        public List<DeltaWrapper> Objects { get; set; }
        Dictionary<int, DeltaWrapper> _baselineState;

        public DataObjectPacket()
        {
            Objects = new List<DeltaWrapper>();
            SnapId = NetTime.SimTick;
        }

        public void SetBaseline(Dictionary<int, DeltaWrapper> state)
        {
            _baselineState = state;
        }

        public override void Deserialize(Bitstream msg)
        {
            SnapId = msg.ReadUInt32();
            BaselineId = msg.ReadUInt32();

            ushort count = msg.ReadByte();
            Dictionary<int, NetworkObject> state = new Dictionary<int, NetworkObject>();
            int addDeltaCount = 0;
            for (int i=0; i < count; ++i)
            {
                ushort objTypeId = msg.ReadByte();
                var entId = msg.ReadVariableUInt32();

                NetworkObject obj = ObjectMapper.Lookup(entId, objTypeId, true);
                if (obj == null)
                {
                    obj = ObjectMapper.Create(entId, objTypeId);
                }

                NetworkObject realObj = ObjectMapper.Lookup(entId, objTypeId, false);
                var realHash = 0;
                if (realObj != null)
                    realHash = realObj.GetHashCode();

                NetworkObject baseline = null;
                var usebaseLine = msg.ReadBool();
                uint bOffset = 0;
                if (usebaseLine)
                {
                    byte offset = msg.ReadByte();
                    var objBaselineId = BaselineId - (uint)offset;
                    bOffset = objBaselineId;

                    baseline = ObjectMapper.GetBaseline(objBaselineId, realHash);
                }


                var ownerType = obj.GetType().GetTypeInfo();
                var dItem = DataLookupTable.Get(ownerType);
                ReadNetObject(dItem, msg, obj, baseline);
                ObjectMapper.AddDeltaState(realHash, SnapId, obj);

                if (BaselineId != 0 && realHash != 0)
                    addDeltaCount++;
            }

            if (addDeltaCount == count)
                ObjectMapper.LastSimId = BaselineId;
        }

        public override void Serialize(Bitstream msg)
        {
          
            msg.Write(SnapId);
            msg.Write(BaselineId);
            msg.Write((byte)Objects.Count);
            foreach(var obj in Objects)
            {
                var ownerType = obj.Object.GetType().GetTypeInfo();
                ushort objTypeId = ObjectMapper.LookupType(ownerType);
                msg.Write((byte)objTypeId);
                msg.WriteVariableUInt32(obj.Lookup);

                var dItem = DataLookupTable.Get(ownerType);

                DeltaWrapper baseline = null;
                if (_baselineState != null)
                {
                    _baselineState.TryGetValue(obj.Object.GetHashCode(), out baseline);
                }

                NetworkObject baselineObj = null;
                if (baseline != null)
                {
                    uint offset = BaselineId - baseline.Lookup;
                    if (offset < Byte.MaxValue && baseline.Object != null)
                    {
                        msg.Write(true);
                        msg.Write((byte)offset);
                        baselineObj = baseline.Object;
                    }
                }

                if (baselineObj == null)
                {
                    msg.Write(false);
                }

                WriteNetObject(dItem, msg, obj.Object, baselineObj);
            }
        }

        private static bool UseBaseline(DataSerializationProperty prop, NetworkObject owner, NetworkObject baseline)
        {
            if (baseline == null) return false;
            switch (prop.Type)
            {
                case DataTypes.BOOL:
                    {
                        bool a = prop.Get<bool>(owner);
                        bool b = prop.Get<bool>(baseline);
                        return a == b;
                    }
                case DataTypes.BYTE:
                    {
                        sbyte a = prop.Get<sbyte>(owner);
                        sbyte b = prop.Get<sbyte>(baseline);
                        return a == b;
                    }
                case DataTypes.DOUBLE:
                    {
                        double a = prop.Get<double>(owner);
                        double b = prop.Get<double>(baseline);
                        return a == b;
                    }
                case DataTypes.FLOAT:
                    {
                        float a = prop.Get<float>(owner);
                        float b = prop.Get<float>(baseline);
                        return a == b;
                    }
                case DataTypes.INT:
                    {
                        int a = prop.Get<int>(owner);
                        int b = prop.Get<int>(baseline);
                        return a == b;
                    }
                case DataTypes.LONG:
                    {
                        long a = prop.Get<long>(owner);
                        long b = prop.Get<long>(baseline);
                        return a == b;
                    }
                case DataTypes.SHORT:
                    {
                        short a = prop.Get<short>(owner);
                        short b = prop.Get<short>(baseline);
                        return a == b;
                    }
                case DataTypes.UBYTE:
                    {
                        byte a = prop.Get<byte>(owner);
                        byte b = prop.Get<byte>(baseline);
                        return a == b;
                    }
                case DataTypes.UINT:
                    {
                        uint a = prop.Get<uint>(owner);
                        uint b = prop.Get<uint>(baseline);
                        return a == b;
                    }
                case DataTypes.ULONG:
                    {
                        ulong a = prop.Get<ulong>(owner);
                        ulong b = prop.Get<ulong>(baseline);
                        return a == b;
                    }
                case DataTypes.USHORT:
                    {
                        ushort a = prop.Get<ushort>(owner);
                        ushort b = prop.Get<ushort>(baseline);
                        return a == b;
                    }
                case DataTypes.VECTOR2:
                    {
                        Vector2 a = prop.Get<Vector2>(owner);
                        Vector2 b = prop.Get<Vector2>(baseline);
                        return a == b;
                    }
            }

            return false;
        }

        private static void ApplyBaseline(DataSerializationProperty prop, NetworkObject owner, NetworkObject clone)
        {
            switch (prop.Type)
            {
                case DataTypes.BOOL:
                    {
                        bool b = prop.Get<bool>(clone);
                        prop.Set(owner, b); break;
                    }
                case DataTypes.BYTE:
                    {
                        sbyte b = prop.Get<sbyte>(clone);
                        prop.Set(owner, b); break;
                    }
                case DataTypes.DOUBLE:
                    {
                        double b = prop.Get<double>(clone);
                        prop.Set(owner, b); break;
                    }
                case DataTypes.FLOAT:
                    {
                        float b = prop.Get<float>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.INT:
                    {
                        int b = prop.Get<int>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.LONG:
                    {
                        long b = prop.Get<long>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.SHORT:
                    {
                        short b = prop.Get<short>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.UBYTE:
                    {
                        byte b = prop.Get<byte>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.UINT:
                    {
                        uint b = prop.Get<uint>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.ULONG:
                    {
                        ulong b = prop.Get<ulong>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.USHORT:
                    {
                        ushort b = prop.Get<ushort>(clone);
                         prop.Set(owner, b); break;
                    }
                case DataTypes.VECTOR2:
                    {
                        Vector2 b = prop.Get<Vector2>(clone);
                         prop.Set(owner, b); break;
                    }
            }
        }

        internal static void ReadNetObject(DataSerializationItem item, Bitstream msg, NetworkObject owner, NetworkObject baseline=null)
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
                    if (baseline != null)
                    {
                        ApplyBaseline(prop, owner, baseline);
                    }
                    else
                    {
                        throw new InvalidOperationException("Can't find baseline!");
                    }
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
                        prop.Set(owner, msg.ReadVariableUInt32());
                        break;
                    case DataTypes.BYTE:
                        prop.Set(owner, msg.ReadSByte());
                        break;
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
                            var o = (NetworkObject)prop.CreateChildProperty();
                            if (o != null)
                            {
                                var child = (NetworkObject)prop.Get<NetworkObject>(baseline);
                                ReadNetObject(childProp, msg, o, child);
                                prop.Set(owner, o);
                            }
                        }
                        break;
                }
            }
        }

        internal static void WriteNetObject(DataSerializationItem item, Bitstream msg, NetworkObject owner, NetworkObject baseline = null)
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
                if (UseBaseline(prop, owner, baseline))
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
                        var b = prop.Get<bool>(owner);
                        msg.Write(b);
                        break;
                    case DataTypes.UBYTE:
                        msg.Write(prop.Get<byte>(owner));
                        break;
                    case DataTypes.UINT:
                        msg.WriteVariableUInt32(prop.Get<uint>(owner));
                        break;
                    case DataTypes.BYTE:
                        msg.Write(prop.Get<sbyte>(owner));
                        break;
                    case DataTypes.INT:
                        msg.WriteVariableInt32(prop.Get<int>(owner));
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

                            var baselinebackup = baseline;
                            if (baseline != null)
                            {
                                var childBase = prop.Get<NetworkObject>(baseline);
                                baseline = childBase;
                            }
                            WriteNetObject(childProp, msg, o, baseline);
                            baseline = baselinebackup;
                        }
                        break;
                }
            }
        }
    }
}
