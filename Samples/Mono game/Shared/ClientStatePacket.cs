using Asgard.Core.Network.Packets;
using System.Collections.Generic;
using Asgard.Core.Network;
using Asgard.Core.System;

namespace Shared
{

    [Packet(110, NetDeliveryMethod.ReliableOrdered)]
    public class ClientStatePacket : Packet
    {
        public int SnapId { get; set; }
        public List<PlayerStateData> State { get; set; }


        public override void Deserialize(Bitstream msg)
        {
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write((ushort)State.Count);
            foreach(var o in State)
            {
                msg.Write(o.Forward);
                msg.Write(o.Back);
                msg.Write(o.Left);
                msg.Write(o.Right);
                msg.Write(o.Position);
                msg.Write(o.SimTick);
            }
            msg.Write(SnapId);
        }
    }
}
