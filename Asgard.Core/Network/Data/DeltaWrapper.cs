using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.Data
{
    public class DeltaWrapper
    {
        public uint Lookup { get; set; }
        public NetworkObject Object { get; set; }
    }

    public class DeltaLookup
    {
        public int Lookup { get; set; }
        public NetworkObject Object { get; set; }
    }

    public class DeltaList
    {
        public bool HasAcked { get; set; }

        public Collections.LinkedList<DeltaWrapper> Objects { get; set; }

        public DeltaList()
        {
            Objects = new Collections.LinkedList<DeltaWrapper>();
        }
    }
}
