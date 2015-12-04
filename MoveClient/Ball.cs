using Artemis;
using Artemis.Manager;
using Asgard.Core.Network.Data;
using Asgard.Core.Physics;
using Asgard.Core.System;
using FarseerPhysics.Collision.Shapes;
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
        public bool _isControlled = false;

        public Ball()
        {
        }

        public override void OnCreated(AsgardBase instance, Entity entity)
        {
            var world = instance.LookupSystem<Midgard>();
            var manager = world.EntityManager;
            var body =world.CreateComponent(entity, BodyDef).Body;

            CircleShape shape = new CircleShape(1, 1);
            var fix = body.CreateFixture(shape);
            fix.Restitution = 1;

        }
    }
}
