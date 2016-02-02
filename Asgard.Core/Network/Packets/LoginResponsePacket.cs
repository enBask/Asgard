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
        public uint OwnerEntityId { get; set; }
        public bool Success { get; set; }

        public override void Deserialize(Bitstream msg)
        {
            OwnerEntityId = msg.ReadUInt32();
            Success = msg.ReadBool();
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write(OwnerEntityId);
            msg.Write(Success);
        }
    }
}
