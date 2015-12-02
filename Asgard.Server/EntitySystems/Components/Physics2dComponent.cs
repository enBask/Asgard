using Artemis.Interface;
using Asgard.Core.Physics;

namespace Asgard.EntitySystems.Components
{
    public class Physics2dComponent :  IComponent
    {

        public BodyDefinition BodyDefinition;
        public Body Body { get; internal set; }
    }
}
