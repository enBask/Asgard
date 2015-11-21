using Artemis;
using Artemis.Attributes;
using Artemis.Interface;
using Asgard.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.EntitySystems.Components
{
    public abstract class PlayerComponent : IComponent
    {
        public NetNode NetworkNode { get; set; }

        public PlayerComponent(NetNode networkNode)
        {
            NetworkNode = networkNode;
        }
    }
}
