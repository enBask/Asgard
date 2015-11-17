using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Packets
{
    public interface IPacket
    {

    }

    public abstract class Packet : IPacket
    {
        public ushort PacketId { get; private set; }
        public NetDeliveryMethod DeliveryMethod { get; protected set; }

        public BitStream Stream
        { get; set; }

        public NetConnection Connection
        { get; set; }

        public double ReceiveTime
        { get; set; }

        public Packet()
        {
            DeliveryMethod = NetDeliveryMethod.Unreliable;
            PacketId = PacketFactory.GetPacketId(GetType());
            Stream = new BitStream();
        }

        public abstract void OnReceiveMessage();
        public abstract void OnSendMessage();

        public NetOutgoingMessage SendMessage(Connection connection)
        {
            OnSendMessage();

            NetPeer peer = connection.Peer;
            NetOutgoingMessage sendMsg = peer.CreateMessage();
            sendMsg.Write((uint)PacketId, 16);
            sendMsg.Write(Stream.GetData());

            return sendMsg;
        }

        public static Packet Get(NetIncomingMessage message)
        {
            var packetId = message.ReadUInt16();
            var packetType = PacketFactory.GetPacketType(packetId);

            var newPacket = (Packet)Activator
                .CreateInstance(packetType);

            if (newPacket != null)
            {
                var stream = newPacket.Stream;
                var data = message.ReadBytes(message.LengthBytes - (int)message.Position);
                stream.SetData(data);
                newPacket.OnReceiveMessage();
                return newPacket;
            }

            //TODO
            return null;
        }
    }
}
