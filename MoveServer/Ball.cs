using Artemis;
using Artemis.Manager;
using Asgard.Core.Network.Data;
using Asgard.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.System;
using FarseerPhysics.Collision.Shapes;

namespace MoveServer
{
    public class Ball : DefinitionNetworkObject
    {
        public NetworkProperty<BodyDefinition> BodyDef { get; set; }
        Entity _entity;

        public Ball()
        {

        }

        public void Setup(Midgard world,  long ballId = 0)
        {
            var manager = world.EntityManager;
            if (ballId > 0)
            {
                _entity = manager.GetEntityByUniqueId(ballId);
                if (_entity == null)
                    _entity = manager.Create(ballId);
            }
            else
            {
                _entity = manager.Create();
            }

            var phyComp = world.CreateComponent(_entity, BodyDef);
            var body = phyComp.Body;

            CircleShape shape = new CircleShape(1,1);
            var fix = body.CreateFixture(shape);
            fix.Restitution = 1;
        }

        public override void OnCreated(AsgardBase instance, Entity entity)
        {

        }
    }
}
