using Asgard.Core.Network.Packets;
using Asgard.Core.Network;
using System.Collections.Generic;

namespace MoveServer
{

    public class PlayerStateData
    {
        public bool Forward { get; set; }
        public bool Back { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        public float RenderX { get; set; }
        public float RenderY { get; set; }

        public int Id { get; set; }
    }

    [Packet(110, NetDeliveryMethod.UnreliableSequenced)]
    public class ClientStatePacket : Packet
    {
        public int SnapId { get; set; }
        public List<PlayerStateData> State { get; set; }


        public override void Deserialize(Bitstream msg)
        {
            State = new List<PlayerStateData>();
            int count = msg.ReadUInt16();
            for (int i = 0; i < count; ++i)
            {
                var o = new PlayerStateData();
                o.Forward = msg.ReadBool();
                o.Back = msg.ReadBool();
                o.Left = msg.ReadBool();
                o.Right = msg.ReadBool();
                o.X = msg.ReadFloat();
                o.Y = msg.ReadFloat();
                o.Id = msg.ReadInt32();
                State.Add(o);
            }
            SnapId = msg.ReadInt32();

        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write((ushort)State.Count);
            foreach (var o in State)
            {
                msg.Write(o.Forward);
                msg.Write(o.Back);
                msg.Write(o.Left);
                msg.Write(o.Right);
                msg.Write(o.X);
                msg.Write(o.Y);
            }
            msg.Write(SnapId);
        }
    }
}
