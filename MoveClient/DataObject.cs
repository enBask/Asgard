using Artemis.Interface;
using Asgard.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveServer
{

    public class DataObject : NetworkObject
    {
        public NetworkProperty<float> X { get; set; }
        public NetworkProperty<float> Y { get; set; }

        public NetworkProperty<float> VelX { get; set; }
        public NetworkProperty<float> VelY { get; set; }

        public float position_error_X { get; set; }
        public float position_error_Y { get; set; }

        public float RenderX { get; set; }
        public float RenderY { get; set; }

        public float PrevX { get; set; }
        public float PrevY { get; set; }

    }
}
