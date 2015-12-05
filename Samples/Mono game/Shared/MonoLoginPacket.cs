using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.Network;

namespace Shared
{
    [Packet(100, NetDeliveryMethod.ReliableOrdered)]
    public class MonoLoginPacket : Packet
    {
        public override void Deserialize(Bitstream msg)
        {
        }

        public override void Serialize(Bitstream msg)
        {
        }
    }
}
