using Asgard.Core.Network.Packets;
using System.Collections.Generic;
using Asgard.Core.Network;
using Asgard.Core.System;
using System;

namespace Shared
{

    [Packet(110, NetDeliveryMethod.ReliableOrdered)]
    public class ClientStatePacket : Packet
    {
        public int SnapId { get; set; }
        public uint AckSyncId { get; set; }
        public List<PlayerStateData> State { get; set; }


        public override void Deserialize(Bitstream msg)
        {
            var count = msg.ReadUInt16();
            count = (ushort)Math.Max((int)count, 0);
            count = (ushort)Math.Min((int)count, 100);
            State = new List<PlayerStateData>(count);
            for(int i =0; i < count; ++i)
            {
                PlayerStateData data = new PlayerStateData();
                data.LeftMouseDown = msg.ReadBool();
                data.MousePositionInWorld = msg.ReadVector2();
                data.Position = msg.ReadVector2();
                data.SimTick = msg.ReadUInt32();
                State.Add(data);
            }
            SnapId = msg.ReadInt32();
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write((ushort)State.Count);
            foreach(var o in State)
            {
                msg.Write(o.LeftMouseDown);
                msg.Write(o.MousePositionInWorld);
                msg.Write(o.Position);
                msg.Write(o.SimTick);
            }
            msg.Write(SnapId);
        }
    }
}
