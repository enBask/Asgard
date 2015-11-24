using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Client.Network
{
    internal class NetConfig
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public int Tickrate { get; set; }
    }
}
