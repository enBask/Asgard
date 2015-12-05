using Asgard.Core.Network.Data;
using Farseer.Framework;

namespace Asgard.Core.Physics
{
    public class BodyDefinition : DefinitionNetworkObject
    {
        public NetworkProperty<Vector2> Position { get; set; }
        public NetworkProperty<float> Angle { get; set; }
        public NetworkProperty<Vector2> LinearVelocity { get; set; }
    }
}
