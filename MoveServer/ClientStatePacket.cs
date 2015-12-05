using Asgard.Core.Network.Packets;
using Asgard.Core.Network;
using System.Collections.Generic;
using Asgard.Core.System;
using Microsoft.Xna.Framework;

namespace MoveServer
{
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

                o.Position = msg.ReadVector2();
                o.SimTick = (uint)msg.ReadInt32();

                State.Add(o);
            }
            SnapId = msg.ReadInt32();

        }

        public override void Serialize(Bitstream msg)
        {           
        }
    }
}
