using Artemis.Interface;
using Asgard.Core.Network.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveServer
{    
    public class DataObject : UnreliableStateSyncNetworkObject
    {
        public NetworkProperty<Vector2> Position { get; set; }
        public NetworkProperty<Vector2> LinearVelocity { get; set; }
    }
}
