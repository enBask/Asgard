using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Packets
{
    enum PacketTypes
    {
        PACKET = 1,
        LOGIN_RESPONSE,
        DATA_OBJECT,
        MAX_PACKET=32
    }
}
