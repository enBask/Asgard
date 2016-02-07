using Asgard.Core.Network.Data;
using Asgard.Core.Network.RPC;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    [Packet((ushort)PacketTypes.RPC, NetDeliveryMethod.ReliableOrdered)]
    class RPCPacket : Packet
    {
        public string Name { get; set; }
        public List<object> Parameters { get; set; }
        public uint EntityId { get; set; }

        public override void Deserialize(Bitstream msg)
        {
            Name = msg.ReadString();
            EntityId = msg.ReadVariableUInt32();
            int count = msg.ReadInt32();
            Parameters = new List<object>(count);
            for(int i=0; i < count; ++i)
            {
                var p = ReadParam(msg);
                Parameters.Add(p);
            }

            RPCManager._Call(Name, EntityId, Parameters);
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write(Name);
            msg.WriteVariableUInt32(EntityId);
            msg.Write(Parameters.Count);
            foreach (var p in Parameters)
            {
                WriteParam(p, msg);
            }
        }

        private Type LookupType(string name)
        {
            var type = Type.GetType(name);
            if (type != null) return type;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var ass in assemblies)
            {
                type = ass.GetType(name);
                if (type != null) return type;
            }

            return null;
        }

        private object ReadParam(Bitstream msg)
        {
            var fullTypeName = msg.ReadString();
            var type = LookupType(fullTypeName);
            var dsitem = new DataSerializationProperty(null, type);
            object o;
            switch (dsitem.Type)
            {
                case DataTypes.BOOL:
                    o = msg.ReadBool();
                    break;
                case DataTypes.UBYTE:
                    o = msg.ReadByte();
                    break;
                case DataTypes.UINT:
                    o = msg.ReadVariableUInt32();
                    break;
                case DataTypes.BYTE:
                    o = msg.ReadSByte();
                    break;
                case DataTypes.INT:
                    o = msg.ReadVariableInt32();
                    break;
                case DataTypes.ULONG:
                    o = msg.ReadUInt64();
                    break;
                case DataTypes.LONG:
                    o = msg.ReadInt64();
                    break;
                case DataTypes.FLOAT:
                    o = msg.ReadFloat();
                    break;
                case DataTypes.DOUBLE:
                    o = msg.ReadDouble();
                    break;
                case DataTypes.STRING:
                    o = msg.ReadString();
                    break;
                case DataTypes.VECTOR2:
                    o = msg.ReadVector2();
                    break;
                default:
                    throw new ArgumentException("Invalid RPC paramter type.");
            }

            return o;

        }

        private static void WriteParam(object o, Bitstream msg)
        {
            var t = o.GetType();
            var dsitem = new DataSerializationProperty(null, t);

            msg.Write(t.FullName);
            switch (dsitem.Type)
            {
                case DataTypes.BOOL:
                    msg.Write((bool)o);
                    break;
                case DataTypes.UBYTE:
                    msg.Write((byte)o);
                    break;
                case DataTypes.UINT:
                    msg.WriteVariableUInt32((uint)o);
                    break;
                case DataTypes.BYTE:
                    msg.Write((sbyte)o);
                    break;
                case DataTypes.INT:
                    msg.WriteVariableInt32((int)o);
                    break;
                case DataTypes.ULONG:
                    msg.Write((ulong)o);
                    break;
                case DataTypes.LONG:
                    msg.Write((long)o);
                    break;
                case DataTypes.FLOAT:
                    msg.Write((float)o);
                    break;
                case DataTypes.DOUBLE:
                    msg.Write((double)o);
                    break;
                case DataTypes.STRING:
                    msg.Write((string)o);
                    break;
                case DataTypes.VECTOR2:
                    msg.Write((Vector2)o);
                    break;
                default:
                    throw new ArgumentException("Invalid RPC paramter type.");
            }
        }

    }
}
