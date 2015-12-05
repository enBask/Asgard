using Artemis;
using Artemis.Manager;
using Asgard.Core.Network.Data;
using Asgard.EntitySystems.Components;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Physics
{
    public class NetPhysicsObject : UnreliableStateSyncNetworkObject
    {
        public NetworkProperty<Vector2> Position { get; set; }
        public NetworkProperty<Vector2> LinearVelocity { get; set; }
        public NetworkProperty<uint> SimTick { get; set; }
        public NetworkProperty<bool> PlayerControlled { get; set; }

        public float position_error_X { get; set; }
        public float position_error_Y { get; set; }
    }


}
