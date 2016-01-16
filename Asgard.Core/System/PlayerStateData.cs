using Asgard.Core.Network;
using Asgard.Core.Network.Data;
using Asgard.EntitySystems.Components;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.System
{
    public class PlayerStateData : NetworkObject
    {
        public NetworkProperty<Vector2> Position { get; set; }
        public NetworkProperty<uint> SimTick { get; set; }

        public PlayerStateData(Physics2dComponent p2Comp)
        {
            if (p2Comp != null && p2Comp.Body != null)
                Position = p2Comp.Body.Position;

            SimTick = NetTime.SimTick;
        }

        public PlayerStateData()
        {

        }
    }
}
