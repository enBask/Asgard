using Asgard.Packets;
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
        private static ConcurrentDictionary<ushort, Type> _PacketLookup =
            new ConcurrentDictionary<ushort, Type>();

        private static ConcurrentDictionary<Type, ushort> _PacketReverseLookup =
            new ConcurrentDictionary<Type, ushort>();

        private static ConcurrentDictionary<ushort, List<Action<IPacket>>> _PacketCallback =
            new ConcurrentDictionary<ushort, List<Action<IPacket>>>();


        private static ushort _nextPacketId = 1;

        public static void AddPacketType<T>()
        {
            _PacketLookup.TryAdd(_nextPacketId, typeof(T));
            _PacketReverseLookup.TryAdd(typeof(T), _nextPacketId++);
        }
        
        public static Type GetPacketType(ushort packetId)
        {
            Type packetType;
            if (_PacketLookup.TryGetValue(packetId, out packetType))
            {
                return packetType;
            }
            else
            {
                //TODO
                return typeof(Packet);
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
