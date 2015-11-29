using Asgard.EntitySystems.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.Network;
using Asgard.Client.Collections;

namespace MoveServer
{
    public class PlayerData : PlayerComponent
    {
        public JitterBuffer<PlayerStateData> InputBuffer { get; set; }
        public PlayerStateData CurrentState { get; set; }
        public PlayerData(NetNode networkNode) : base(networkNode)
        {
            InputBuffer = new JitterBuffer<PlayerStateData>(60f);
        }

        public PlayerStateData GetNextState()
        {
            var states = InputBuffer.Get();
            if (states != null && states.Count > 0)
            {
                CurrentState = states[0];
            }
            else
            {
                LogHelper.Log("empty buffer");
            }

            return CurrentState;
        }
    }
}
