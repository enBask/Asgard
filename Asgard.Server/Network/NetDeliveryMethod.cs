using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Network
{
    public enum NetDeliveryMethod : byte
    {
        Unknown = 0,
        Unreliable = 1,
        UnreliableSequenced = 2,
        ReliableUnordered = 34,
        ReliableSequenced = 35,
        ReliableOrdered = 67
    }
}
