using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.Network;
using System.Diagnostics;
using Asgard.Core.Interpolation;

namespace MoveServer
{

    [Packet(100, NetDeliveryMethod.UnreliableSequenced)]
    public class SnapshotPacket : Packet, IInterpolationPacket<MoveData>
    {
        public uint Id { get; set; }

        public List<MoveData> DataPoints { get; set; }

        public override void Deserialize(Bitstream msg)
        {           
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write((int)Id);
            msg.Write(DataPoints.Count);
            foreach (var point in DataPoints)
            {
                msg.Write(point.Id);
                msg.Write(point.X);
                msg.Write(point.Y);
                msg.Write(point.VelX);
                msg.Write(point.VelY);
            }

        }
    }
}
