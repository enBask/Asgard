using Artemis.Interface;
using Asgard.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveServer
{    
    public class DataObject : UnreliableNetworkObject
    {
        public NetworkProperty<float> X { get; set; }
        public NetworkProperty<float> Y { get; set; }

        public NetworkProperty<float> VelX { get; set; }
        public NetworkProperty<float> VelY { get; set; }

    }
}
