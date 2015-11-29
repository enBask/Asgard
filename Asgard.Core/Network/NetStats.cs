using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network
{
    public class NetStats
    {
        public float BytesInPerSec { get; set; }
        public float BytesOutPerSec { get; set; }
        public float AvgPing { get; set; }
    }
}
