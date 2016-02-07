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
        public NetworkProperty<float> Rotation { get; set; }
        public NetworkProperty<uint> SimTick { get; set; }

        NetworkProperty<bool> _pControlled;
        public NetworkProperty<bool> PlayerControlled
        {
            get
            {
                return _pControlled;
            }
            set
            {
                _pControlled = value;
            }
        }

        public Vector2 position_error { get; set; }
        public float rotation_error { get; set; }
        public float rotation_slerp { get; set; }
    }


}
