using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono_Server
{
    public class MapRender
    {
        Sprite _groundSprite;
        Sprite _wallSprite;

        public MapRender(Texture2D texture)
        {
            _groundSprite = new Sprite(texture);
            _groundSprite.Origin = new Vector2(0,0);

            _wallSprite = new Sprite(texture);

        }
    }
}
