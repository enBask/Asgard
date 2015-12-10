using Asgard.Core.Network;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    public interface IPacket
    {

    }

    public abstract class Packet : IPacket
    {
        public ushort PacketId { get; private set; }

        public Network.NetDeliveryMethod Method { get; set; }
        public NetNode Connection
        { get; set; }

        public double ReceiveTime
        { get; set; }


        public Packet()
        {
            PacketId = PacketFactory.GetPacketId(GetType());
            Method = PacketFactory.GetPacketType(PacketId).Method;
        }

        public abstract void Deserialize(Bitstream msg);
        public abstract void Serialize(Bitstream msg);

        public NetOutgoingMessage SendMessage(Connection connection)
        {
            NetPeer peer = connection.Peer;
            NetOutgoingMessage sendMsg = peer.CreateMessage();
            sendMsg.Write((uint)PacketId, 16);

            Bitstream stream = new Bitstream(sendMsg);
            Serialize(stream);
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
                Bitstream stream = new Bitstream(message);
                newPacket.Deserialize(stream);
                return newPacket;
            }

            //TODO
            return null;
        }
    }
}
