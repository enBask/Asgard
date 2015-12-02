using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Physics
{
    public static class StructHelpers
    {
        public static float crossProduct(this Vector2 v, Vector2 v2)
        {
            return (v.X * v2.Y - v.Y * v2.X);
        }
    }

    public struct AABB
    {
        public Vector2 Upper;
        public Vector2 Lower;

        public AABB(Vector2 upper, Vector2 lower)
        {
            Upper = upper;
            Lower = lower;
        }

        public AABB(float x1, float y1, float x2, float y2)
        {
            Upper = new Vector2(x1, y1);
            Lower = new Vector2(x2, y2);

            if (Extents().X == 0 || Extents().Y == 0)
            {
                throw new ArgumentException("invalid AABB region");
            }
        }

        public Vector2 Extents()
        {
            return new Vector2(Math.Abs(Upper.X - Lower.X) / 2f,
                Math.Abs(Upper.Y - Lower.Y)  / 2f);
        }

        public float Area()
        {
            return Math.Abs((Upper.Y - Lower.Y) * (Upper.X - Lower.X)); 
        }
        
        public Vector2 Max()
        {
            return new Vector2(Math.Max(Upper.X, Lower.X), Math.Max(Upper.Y, Lower.Y));
        }

        public Vector2 Min()
        {
            return new Vector2(Math.Min(Upper.X, Lower.X), Math.Min(Upper.Y, Lower.Y));
        }


        public bool PointIn(Vector2 point)
        {
            var max = Max();
            var min = Min();
            if (point.X > max.X) return false;
            if (point.X < min.X) return false;
            if (point.Y > max.Y) return false;
            if (point.Y < min.Y) return false;

            return true;
        }
    }
}
