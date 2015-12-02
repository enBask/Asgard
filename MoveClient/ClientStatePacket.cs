using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.Network;
using MoveClient;
using System.Diagnostics;
using Asgard.Client.Collections;
using Asgard.Core.Interpolation;
using Asgard.Core.System;

namespace MoveServer
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
                msg.Write(o.Position.X);
                msg.Write(o.Position.Y);
                msg.Write(0);
            }
            msg.Write(SnapId);
        }
    }
}
