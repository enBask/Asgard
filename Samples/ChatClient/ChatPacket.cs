using Asgard.Network;
using Asgard.Packets;
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
        public string From { get; set; }
        public override void Deserialize(Bitstream msg)
        {
            Message = msg.ReadString();
            From = msg.ReadString();
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write(Message);
            msg.Write(From);
        }
    }
}
