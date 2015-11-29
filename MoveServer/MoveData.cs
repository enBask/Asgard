using Artemis.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveServer
{
    public class MoveData : IComponent
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }

        public int SnapId { get; set; }
        public MoveData(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
