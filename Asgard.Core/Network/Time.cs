using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network
{
    public static class NetTime
    {
        private static double _simDiff;

        public static uint SimTick
        {
            get; set;
        }

        public static float SimTime
        {
            get
            {
                return (float)(Lidgren.Network.NetTime.Now + _simDiff);
            }
        }

        public static float RealTime
        {
            get
            {
                return (float)(Lidgren.Network.NetTime.Now);
            }
        }

        public static void SetSimTime(double time)
        {
            _simDiff = time;
        }
    }
}
