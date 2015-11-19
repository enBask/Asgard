using Asgard.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network
{
    public class Bootstrap
    {
        public static void Init()
        {
            InitPacketTypes();
        }

        private static void InitPacketTypes()
        {
            PacketTypeScanner.BuildPacketTypes();
        }
    }
}
