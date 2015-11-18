using Asgard.Network;
using Asgard.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    [Packet(101, NetDeliveryMethod.ReliableUnordered)]
    public class ChatLoginPacket : Packet
    {
        public string Username { get; set; }
        public override void Deserialize(Bitstream msg)
        {
            Username = msg.ReadString();
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write(Username);
        }
    }
}
