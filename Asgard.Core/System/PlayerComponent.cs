using Artemis;
using Artemis.Attributes;
using Artemis.Interface;
using Asgard.Core.Collections;
using Asgard.Core.Network;
using Asgard.Core.Physics;
using Asgard.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.EntitySystems.Components
{
    public class PlayerComponent : IComponent
    {
        public NetNode NetworkNode { get; set; }
        public Body Body { get; set; }

        public JitterBuffer<PlayerStateData> InputBuffer { get; set; }
        public PlayerStateData CurrentState { get; set; }


        public PlayerComponent(NetNode networkNode)
        {
            NetworkNode = networkNode;
            InputBuffer = new JitterBuffer<PlayerStateData>(30);
        }

        public PlayerStateData GetNextState()
        {
            var states = InputBuffer.Get();
            if (states != null && states.Count > 0)
            {
                CurrentState = states[0];
            }

            return CurrentState;
        }
    }
}
