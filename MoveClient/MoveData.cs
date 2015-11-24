using Artemis.Interface;
using Asgard.Core.Interpolation;

namespace MoveServer
{
    public class MoveData : IComponent
    {
        public int Id { get; set; }

        [Interpolation]
        public float X { get; set; }
        [Interpolation]
        public float Y { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }

        public MoveData() : this(0,0,0,0)
        {

        }

        public MoveData(float x, float y, float velx, float vely)
        {
            X = x;
            Y = y;
            VelX = velx;
            VelY = vely;
        }
    }
}
