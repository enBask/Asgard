using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Packets
{
    public abstract class LoginRequestPacket : Packet
    {
        public bool IsValid { get; protected set; }
    }

    public class LoginResponsePacket : Packet
    {
        public override void OnReceiveMessage()
        {
        }

        public override void OnSendMessage()
        {
        }
    }
}
