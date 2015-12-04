using Artemis;
using Artemis.Manager;
using Asgard.Core.Network.Data;
using Asgard.Core.Physics;
using Asgard.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveServer
{
    public class Ball : DefinitionNetworkObject
    {
        NetworkProperty<BodyDefinition> BodyDef { get; set; }
        Entity _entity;
        public bool _isControlled = false;

        public Ball()
        {
        }

        public override void OnCreated(AsgardBase instance, Entity entity)
        {
            var world = instance.LookupSystem<Midgard>();
            var manager = world.EntityManager;
            world.CreateComponent(entity, BodyDef);
        }
    }
}
