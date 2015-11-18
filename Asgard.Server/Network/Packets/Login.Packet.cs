using Asgard.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Packets
{
    public abstract class LoginRequestPacket : Packet
    {
        public bool IsValid { get; protected set; }
    }

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
