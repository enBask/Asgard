using Asgard.EntitySystems.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.Network;

namespace MoveServer
{
    public class PlayerData : PlayerComponent
    {
        public PlayerData(NetNode networkNode) : base(networkNode)
        {
        }
    }
}
