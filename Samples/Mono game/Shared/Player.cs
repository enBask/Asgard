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

namespace Shared
{
    public class RenderData : DefinitionNetworkObject
    {
        static public ContentManager Content;
        public bool MovingToPosition { get; set; }

        Entity _owner;

        Sprite _sprite;
        float _worldSize = 10f;
        Random rnd = new Random();
        Farseer.Framework.Vector2 _speed = new Farseer.Framework.Vector2(1f,1f);

        public RenderData()
        {

        }

        public void Set(Midgard midgard, Entity entity, ContentManager manager)
        {
            var text = manager.Load<Texture2D>("roguelikeChar_transparent");

            TextureRegion2D region = new TextureRegion2D(text, 0, 0, 16, 16);
            _sprite = new Sprite(region);
            _sprite.Scale = new Vector2(1, 1);
            _owner = entity;
            CreatePhysics(midgard, entity);

        }

        public void SetSpeed(Entity entity, Farseer.Framework.Vector2 direction)
        {
            var phyComp = entity.GetComponent<Physics2dComponent>();
            if (phyComp == null || phyComp.Body == null) return;

            phyComp.Body.LinearVelocity = (_speed * direction);
        }

        public RenderData(Midgard midgard, Entity entity, ContentManager manager)
        {
            var text = manager.Load<Texture2D>("roguelikeChar_transparent");

            TextureRegion2D region = new TextureRegion2D(text, 0, 0, 16, 16);
            _sprite = new Sprite(region);
            _sprite.Scale = new Vector2(1, 1);

            CreatePhysics(midgard, entity);

        }

        private void CreatePhysics(Midgard midgard, Entity entity, bool usePos = true)
        {
            var comp = midgard.CreateComponent(entity,
                            new BodyDefinition()
                            {
                                Position = usePos ? new Farseer.Framework.Vector2(18f + (float)rnd.Next(0, 4), 17f + (float)rnd.Next(0, 3)) :
                                new Farseer.Framework.Vector2(0, 0)
                            });


            Vertices rectangleVertices = PolygonTools.CreateRectangle(0.5f, 0.2f,
                new Farseer.Framework.Vector2(0f, 0.4f), 0f);
            PolygonShape shape = new PolygonShape(rectangleVertices, 10000f);
            var fix = comp.Body.CreateFixture(shape);
            comp.Body.FixedRotation = true;
            comp.Body.Mass = 1000f;
            comp.Body.Restitution = 1f;
            comp.Body.CollisionCategories = FarseerPhysics.Dynamics.Category.Cat2;
            comp.Body.CollidesWith = FarseerPhysics.Dynamics.Category.Cat1;
        }

        public override void OnDestroyed(AsgardBase instance, Entity entity, bool destoryEntity=true)
        {
            var midgard = instance.LookupSystem<Midgard>();
            var phyComp = entity.GetComponent<Physics2dComponent>();
            if (phyComp != null && phyComp.Body != null)
            {
                midgard.DeleteBody(phyComp);
            }

            ObjectMapper.DestroyEntity(entity, destoryEntity);
        }

        public override void OnCreated(AsgardBase instance, Entity entity)
        {
            var text = Content.Load<Texture2D>("roguelikeChar_transparent");

            TextureRegion2D region = new TextureRegion2D(text, 0, 0, 16, 16);
            _sprite = new Sprite(region);
            _sprite.Scale = new Vector2(1, 1);
            _owner = entity;


            var midgard = instance.LookupSystem<Midgard>();
            CreatePhysics(midgard, entity, false);
        }

        public void UpdateFromPhysics()
        {
            if (_owner == null) return;
            var dataObj = _owner.GetComponent<NetPhysicsObject>();
            var phyComp = _owner.GetComponent<Physics2dComponent>();
            var playerComp = _owner.GetComponent<PlayerComponent>();


            if (playerComp != null)
            {
                _sprite.Position = new Vector2(playerComp.RenderPosition.X, playerComp.RenderPosition.Y);
            }
            else
            {
                if (dataObj == null) return;
                _sprite.Position = new Vector2(phyComp.Body.Position.X + dataObj.position_error.X ,
                    phyComp.Body.Position.Y + dataObj.position_error.Y);

            }

            _sprite.Position *= _worldSize;
        }

        public Vector2 GetPosition()
        {
            if (_sprite == null)
            {
                return Vector2.Zero;
            }

            return _sprite.Position;
        }

        public void Draw(SpriteBatch batch)
        {
            if (_sprite == null) return;
            batch.Draw(_sprite);
        }
    }
}
