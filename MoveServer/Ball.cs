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

            world.CreateComponent(_entity, BodyDef);
        }

        public override void OnCreated(AsgardBase instance, Entity entity)
        {

        }
    }
}
