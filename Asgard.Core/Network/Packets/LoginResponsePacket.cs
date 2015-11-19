using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    [Packet((ushort)PacketTypes.LOGIN_RESPONSE, NetDeliveryMethod.ReliableUnordered)]
    public class LoginResponsePacket : Packet
    {
        public override void Deserialize(Bitstream msg)
        {
        }

        public override void Serialize(Bitstream msg)
        {
        }
    }
}
