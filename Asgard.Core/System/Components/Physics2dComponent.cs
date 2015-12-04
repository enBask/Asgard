using Artemis.Interface;
using Asgard.Core.Physics;
using FarseerPhysics.Dynamics;

namespace Asgard.EntitySystems.Components
{
    public class Physics2dComponent :  IComponent
    {
        public Body Body { get; internal set; }
    }
}
