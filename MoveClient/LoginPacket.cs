using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    [Packet(101, NetDeliveryMethod.ReliableUnordered)]
    public class MoveLoginPacket : Packet
    {
        public override void Deserialize(Bitstream msg)
        {
        }

        public override void Serialize(Bitstream msg)
        {
        }
    }
}
