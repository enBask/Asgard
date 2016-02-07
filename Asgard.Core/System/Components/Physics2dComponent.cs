using Artemis.Interface;
using Asgard.Core.Physics;
using Farseer.Framework;
using FarseerPhysics.Dynamics;

namespace Asgard.EntitySystems.Components
{
    public class Physics2dComponent :  IComponent
    {
        public Body Body { get; internal set; }
        public Vector2 PreviousLinaryVelocity { get; set; }
    }
}
