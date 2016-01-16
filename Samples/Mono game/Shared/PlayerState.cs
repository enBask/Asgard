using Asgard.Core.Network.Data;
using Asgard.Core.System;
using Asgard.EntitySystems.Components;
using Farseer.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PlayerState : PlayerStateData
    {
        public NetworkProperty<bool> Forward { get; set; }
        public NetworkProperty<bool> Back { get; set; }
        public NetworkProperty<bool> Left { get; set; }
        public NetworkProperty<bool> Right { get; set; }
        public NetworkProperty<bool> LeftMouseDown { get; set; }
        public NetworkProperty<Vector2> MousePositionInWorld { get; set; }

        public PlayerState()
        {

        }
        public PlayerState(Physics2dComponent p2Comp) : base(p2Comp)
        {

        } 
    }
}
