using Asgard.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsoleServer
{
    class MyLoginPacket : LoginRequestPacket
    {
        public override void OnReceiveMessage()
        {
            IsValid = true;
        }

        public override void OnSendMessage()
        {
        }
    }
}
