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

        public NetDeliveryMethod Method { get; private set; }
        public NetConnection Connection
        { get; set; }

        public double ReceiveTime
        { get; set; }


        public Packet()
        {
            PacketId = PacketFactory.GetPacketId(GetType());
            Method = PacketFactory.GetPacketType(PacketId).Method;
        }

        public abstract void Deserialize(NetIncomingMessage msg);
        public abstract void Serialize(NetOutgoingMessage msg);

        public NetOutgoingMessage SendMessage(Connection connection)
        {
            NetPeer peer = connection.Peer;
            NetOutgoingMessage sendMsg = peer.CreateMessage();
            sendMsg.Write((uint)PacketId, 16);

            Serialize(sendMsg);
            return sendMsg;
        }

        public static Packet Get(NetIncomingMessage message)
        {
            var packetId = message.ReadUInt16();
            var pdata = PacketFactory.GetPacketType(packetId);

            var newPacket = (Packet)Activator
                .CreateInstance(pdata.PacketType);

            if (newPacket != null)
            {
                newPacket.Deserialize(message);
                return newPacket;
            }

            //TODO
            return null;
        }
    }
}
