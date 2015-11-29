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

        public float position_error_X { get; set; }
        public float position_error_Y { get; set; }

        public uint SnapId { get; set; }
        public bool Forward { get; set; }
        public bool Back { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }

        public int RemoveSnapId { get; set; }

        public MoveData() : this(0,0)
        {

        }

        public MoveData(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
