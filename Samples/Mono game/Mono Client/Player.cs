using Artemis.Interface;
using Asgard.Core.Network.Data;
using Asgard.Core.Physics;
using Asgard.EntitySystems.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis;
using Asgard.Core.System;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;

namespace Mono_Server
{
    public class RenderData : DefinitionNetworkObject
    {
        static public ContentManager Content;

        Sprite _sprite;
        float _worldSize = 10f;

        public RenderData()
        {

        }

        public RenderData(Midgard midgard, Entity entity, ContentManager manager)
        {
            var text = manager.Load<Texture2D>("roguelikeChar_transparent");

            TextureRegion2D region = new TextureRegion2D(text, 0, 0, 16, 16);
            _sprite = new Sprite(region);
            _sprite.Scale = new Vector2(1, 1);

            CreatePhysics(midgard, entity);

        }

        private void CreatePhysics(Midgard midgard, Entity entity)
        {
            var comp = midgard.CreateComponent(entity,
                            new BodyDefinition()
                            {
                                Position = new Farseer.Framework.Vector2(20f, 17f),
                                LinearVelocity = new Farseer.Framework.Vector2(1f, 1f)
                            });


            Vertices rectangleVertices = PolygonTools.CreateRectangle(0.5f, 0.8f,
                new Farseer.Framework.Vector2(0f, 0.2f), 0f);
            PolygonShape shape = new PolygonShape(rectangleVertices, 1f);
            comp.Body.CreateFixture(shape);
            comp.Body.FixedRotation = true;
        }

        public override void OnCreated(AsgardBase instance, Entity entity)
        {
            var text = Content.Load<Texture2D>("roguelikeChar_transparent");

            TextureRegion2D region = new TextureRegion2D(text, 0, 0, 16, 16);
            _sprite = new Sprite(region);
            _sprite.Scale = new Vector2(1, 1);


            var midgard = instance.LookupSystem<Midgard>();
            CreatePhysics(midgard, entity);
        }

        public void UpdateFromPhysics(Physics2dComponent comp)
        {
            if (comp.Body == null) return;
            _sprite.Position = new Vector2(comp.Body.Position.X * _worldSize, comp.Body.Position.Y * _worldSize);

        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_sprite);
        }
    }
}
