using Asgard.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Utils
{
    internal static class NetworkObjectBuilder
    {
        public static void Compile()
        {
            AssemblyScanner.Execute<NetworkObject>(DataLookupTable.AddType, DataLookupTable.CheckType);
        }
    }
}
