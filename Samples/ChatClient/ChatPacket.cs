using Asgard.Packets;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    [Packet(100, NetDeliveryMethod.ReliableOrdered)]
    public class ChatPacket : Packet
    {
        public string Message { get; set; }
        public override void Deserialize(NetIncomingMessage msg)
        {
            Message = msg.ReadString();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(Message);          
        }
    }
}
