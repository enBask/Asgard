using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Server.Packets
{
    public abstract class LoginRequestPacket : Packet
    {
        public bool IsValid { get; protected set; }
    }

    
}
