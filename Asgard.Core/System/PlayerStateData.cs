using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.System
{
    public class PlayerStateData
    {
        public bool Forward { get; set; }
        public bool Back { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public Vector2 Position { get; set; }
        public uint SimTick { get; set; }
    }
}
