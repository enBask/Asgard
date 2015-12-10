using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    [Packet((ushort)PacketTypes.ACK_STATE_TICK, NetDeliveryMethod.UnreliableSequenced)]
    class AckStatePacket : Packet
    {
        public uint SimId;

        public override void Deserialize(Bitstream msg)
        {
            SimId = msg.ReadUInt32();
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write(SimId);
        }
    }
}
