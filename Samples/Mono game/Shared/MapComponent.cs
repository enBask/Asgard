using Artemis.Interface;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Maps.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class MapComponent : IComponent
    {
        public TiledMap Map { get; set; }
        public GraphicsDevice Device { get; set; }
        public Texture2D Texture { get; set; }

    }
}
