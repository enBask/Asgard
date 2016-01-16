using Asgard.Core.Network.Data;
using Asgard.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    [Packet(110, NetDeliveryMethod.ReliableOrdered)]
    public class ClientStatePacket : Packet
    {
        public List<PlayerStateData> State { get; set; }
        public PlayerStateData PreviousState { get; set; }
        public uint PlayerId { get; set; }

        private static Type _ownerType = null;
        private static DataSerializationItem _lookupItem = null;

        public override void Deserialize(Bitstream msg)
        {
            var count = msg.ReadUInt16();
            count = (ushort)Math.Max((int)count, 0);
            count = (ushort)Math.Min((int)count, 100);
            State = new List<PlayerStateData>(count);

            if (count > 0)
            {
                ushort objType = msg.ReadByte();
                PlayerId = msg.ReadVariableUInt32();
                if (_ownerType == null)
                {
                    _ownerType = ObjectMapper.LookupType(objType);
                    _lookupItem = DataLookupTable.Get(_ownerType.GetTypeInfo());

                }
            }

            NetworkObject pState = PlayerId > 0 ? ObjectMapper.GetCurrentPlayerState(PlayerId) : null;
            for (int i = 0; i < count; ++i)
            {
                NetworkObject data = Activator.CreateInstance(_ownerType) as NetworkObject;
                DataObjectPacket.ReadNetObject(_lookupItem, msg, data, pState);
                pState = data;
                State.Add(data as PlayerStateData);
            }

            if (State.Count > 0 && PlayerId > 0)
                ObjectMapper.SetCurrentPlayerState(PlayerId, State.Last());
        }

        public override void Serialize(Bitstream msg)
        {
            var count = State.Count;
            msg.Write((ushort)State.Count);
            if (count > 0 && _ownerType == null)
            {
                _ownerType = State[0].GetType();
                _lookupItem = DataLookupTable.Get(_ownerType.GetTypeInfo());
            }
            if (count > 0)
            {
                ushort objTypeId = ObjectMapper.LookupType(_ownerType);
                msg.Write((byte)objTypeId);
                msg.WriteVariableUInt32(PlayerId);
            }

            PlayerStateData pState = PreviousState;
            foreach (var o in State)
            {
                DataObjectPacket.WriteNetObject(_lookupItem, msg, o, pState);
                pState = o;
            }
        }
    }

}
