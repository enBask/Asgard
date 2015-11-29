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

namespace MoveServer
{   

    [Packet(100, NetDeliveryMethod.UnreliableSequenced)]
    public class SnapshotPacket : Packet, IInterpolationPacket<MoveData>
    {
        public uint Id { get; set; }

        public List<MoveData> DataPoints { get; set; }

        public override void Deserialize(Bitstream msg)
        {
            Id = (uint)msg.ReadInt32();
            var count = msg.ReadUInt16();
            DataPoints = new List<MoveData>(count);

            for (int i = 0; i < count; ++i)
            {
                MoveData data = new MoveData();
                data.Id = msg.ReadInt32();
                data.X = msg.ReadFloat();
                data.Y = msg.ReadFloat();
                data.VelX = msg.ReadFloat();
                data.VelY = msg.ReadFloat();
                data.RemoveSnapId = msg.ReadInt32();
                DataPoints.Add(data);
            }
        }

        public override void Serialize(Bitstream msg)
        {           
        }
    }
}
