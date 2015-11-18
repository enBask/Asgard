using Asgard.Packets;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsoleServer
{
    [Packet(100, NetDeliveryMethod.ReliableUnordered)]
    class MyLoginPacket : LoginRequestPacket
    {
        public string Username { get; set; }
        public override void Deserialize(NetIncomingMessage msg)
        {
            Username = msg.ReadString();
            IsValid = true;
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(Username);
        }
    }
}
