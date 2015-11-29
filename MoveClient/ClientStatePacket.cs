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

    [Packet(110, NetDeliveryMethod.ReliableOrdered)]
    public class ClientStatePacket : Packet
    {
        public int SnapId { get; set; }
        public List<PlayerStateData> State { get; set; }


        public override void Deserialize(Bitstream msg)
        {
            State = new List<PlayerStateData>();
            int count = msg.ReadUInt16();
            for(int i = 0; i < count; ++i)
            {
                var o = new PlayerStateData();
                o.Forward = msg.ReadBool();
                o.Back = msg.ReadBool();
                o.Left = msg.ReadBool();
                o.Right = msg.ReadBool();
                State.Add(o);
            }
            SnapId = msg.ReadInt32();

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
                msg.Write(o.X);
                msg.Write(o.Y);
                msg.Write(o.Id);
            }
            msg.Write(SnapId);
        }
    }
}
