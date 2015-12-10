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
        public List<PlayerStateData> State { get; set; }


        public override void Deserialize(Bitstream msg)
        {
            var count = msg.ReadUInt16();
            count = (ushort)Math.Max((int)count, 0);
            count = (ushort)Math.Min((int)count, 100);
            State = new List<PlayerStateData>(count);

            PlayerStateData pState = null;
            for(int i =0; i < count; ++i)
            {
                PlayerStateData data = new PlayerStateData();
                var dup = msg.ReadBool();

                if (dup)
                {
                    data.SimTick = msg.ReadUInt32();
                    data.Position = pState.Position;
                    data.MousePositionInWorld = pState.MousePositionInWorld;
                    data.LeftMouseDown = pState.LeftMouseDown;
                    State.Add(data);
                }
                else
                {
                    data.LeftMouseDown = msg.ReadBool();
                    data.MousePositionInWorld = msg.ReadVector2();
                    data.Position = msg.ReadVector2();
                    data.SimTick = msg.ReadUInt32();
                    State.Add(data);
                    pState = data;
                }
            }
        }

        public override void Serialize(Bitstream msg)
        {
            msg.Write((ushort)State.Count);
            PlayerStateData pState = null;
            foreach(var o in State)
            {
                if (pState != null && 
                    pState.Position == o.Position &&
                    pState.LeftMouseDown == o.LeftMouseDown &&
                    pState.MousePositionInWorld == o.MousePositionInWorld)
                {
                    msg.Write(true);
                    msg.Write(o.SimTick);
                }
                else
                {
                    pState = o;
                    msg.Write(false);
                    msg.Write(o.LeftMouseDown);
                    msg.Write(o.MousePositionInWorld);
                    msg.Write(o.Position);
                    msg.Write(o.SimTick);
                }               
            }
        }
    }
}
