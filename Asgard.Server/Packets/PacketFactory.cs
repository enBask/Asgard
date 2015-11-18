using Asgard.Packets;
using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard
{
    public class PacketFactory
    {
        public class PacketData
        {
            public ushort Id { get; set;}
            public NetDeliveryMethod Method { get; set; }
            public Type PacketType { get; set; }
        }


        private const ushort maxInternalPacketId = (ushort)PacketTypes.MAX_PACKET;
        private static ConcurrentDictionary<ushort, PacketData> _PacketLookup =
            new ConcurrentDictionary<ushort, PacketData>();

        private static ConcurrentDictionary<Type, ushort> _PacketReverseLookup =
            new ConcurrentDictionary<Type, ushort>();

        private static ConcurrentDictionary<ushort, List<Action<IPacket>>> _PacketCallback =
            new ConcurrentDictionary<ushort, List<Action<IPacket>>>();

  
        public static void AddPacketType<T>(ushort packetId, NetDeliveryMethod method)
        {
            AddPacketType(typeof(T), packetId, method);
        }

        public static void AddPacketType(Type packetType, ushort packetId, NetDeliveryMethod method)
        {
            if (packetId <= maxInternalPacketId)
            {
                throw new ArgumentException("packet id falls in reserved range.");
            }
            if (_PacketLookup.ContainsKey(packetId))
            {
                throw new ArgumentException("packet id in use");
            }

            PacketData pdata = new PacketData()
            {
                Id = packetId,
                Method = method,
                PacketType = packetType
            };

            _PacketLookup.TryAdd(packetId, pdata);
            _PacketReverseLookup.TryAdd(packetType, packetId);
        }
        
        public static PacketData GetPacketType(ushort packetId)
        {
            PacketData pdata;
            if (_PacketLookup.TryGetValue(packetId, out pdata))
            {
                return pdata;
            }
            else
            {
                return null;
            }
        }

        public static ushort GetPacketId(Type packetType)
        {
            ushort packetId;
            if (_PacketReverseLookup.TryGetValue(packetType, out packetId))
            {
                return packetId;
            }
            else
            {
                //TODO
                return 0; //maps to base Packet class
            }
        }

        public static ushort GetPacketId<T>()
        {
            return GetPacketId(typeof(T));
        }

        public static void AddCallback<T>(Action<T> callback) where T : Packet
        {
            var packetId = GetPacketId<T>();

            List<Action<IPacket>> callbackList = new List<Action<IPacket>>();
            callbackList.Add((Action<IPacket>)callback);

            _PacketCallback.AddOrUpdate(packetId, callbackList, (pId, pList) =>
            {

                pList.Add((Action<IPacket>)callback);
                return pList;
            });
        }

        public static void RaiseCallbacks(Packet packet)
        {
            var packetId = packet.PacketId;
            List<Action<IPacket>> callbackList;
            if (_PacketCallback.TryGetValue(packetId, out callbackList))
            {
                foreach(var callback in callbackList)
                {
                    callback(packet);
                }
            }
        }
        
    }
}
