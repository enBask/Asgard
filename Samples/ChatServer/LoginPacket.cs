using Asgard.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace ChatServer
{
    [Packet(101, Lidgren.Network.NetDeliveryMethod.ReliableUnordered)]
    public class ChatLoginPacket : Packet
    {
        public string Username { get; set; }
        public override void Deserialize(NetIncomingMessage msg)
        {
            Username = msg.ReadString();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(Username);
        }
    }
}
