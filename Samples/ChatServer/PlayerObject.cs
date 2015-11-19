using Asgard.EntitySystems.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.Network;

namespace ChatServer
{
    public class PlayerObject : PlayerComponent
    {
        public string DisplayName { get; set; }
        public PlayerObject(NetNode networkNode, string displayName) : base(networkNode)
        {
            DisplayName = displayName;
        }
    }
}
