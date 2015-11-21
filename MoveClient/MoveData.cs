using Artemis.Interface;
using MoveClient;
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

        [Interpolation]
        public double X { get; set; }
        [Interpolation]
        public double Y { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }

        public MoveData() : this(0,0,0,0)
        {

        }

        public MoveData(double x, double y, double velx, double vely)
        {
            X = x;
            Y = y;
            VelX = velx;
            VelY = vely;
        }
    }
}
