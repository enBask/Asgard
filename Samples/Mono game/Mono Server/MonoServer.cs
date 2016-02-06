using MonoGame.Extended;
using MonoGame.Extended.Maps.Tiled;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System;
using Asgard.Core.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using Artemis;
using Asgard.EntitySystems.Components;
using Shared;
using Asgard.Core.System;
using MonoGame.Extended.BitmapFonts;
using Asgard;
using Asgard.Core.Network;

namespace Mono_Server
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MonoServer : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameServer _gameServer;
        Camera2D _camera;
        float _worldfactor = 1f / 10f;
        Texture2D _lineTexture;
        BitmapFont _font;

        Entity _mapEntity;

        public MonoServer()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            IsMouseVisible = true;

            _camera = new Camera2D(GraphicsDevice);
            _camera.Zoom = 2.0f;
            _camera.Position = new Vector2(-120,-5);

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            IsFixedTimeStep = true;
            _gameServer = new GameServer(this);
            _gameServer.Start();

            _lineTexture = new Texture2D(GraphicsDevice, 1, 1);
            _lineTexture.SetData<Color>(
            new Color[] { Color.White });// fill the texture with white

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            var viewMap = Content.Load<TiledMap>("new_map");
            _mapEntity = ObjectMapper.CreateEntityById();
            var mapComponent = new MapComponent();
            mapComponent.Map = viewMap;
            mapComponent.Device = GraphicsDevice;
            mapComponent.Texture = Content.Load<Texture2D>("roguelikeSheet_transparent");
            _mapEntity.AddComponent(mapComponent);

            var mapData = (MapData)ObjectMapper.Create((uint)_mapEntity.UniqueId, typeof(MapData));
            mapData.Load(_gameServer, viewMap);

            _font = Content.Load<BitmapFont>("hack_font");
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _gameServer.Tick(gameTime.ElapsedGameTime.TotalSeconds);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            var viewmap = _mapEntity.GetComponent<MapComponent>().Map;
            viewmap.Draw(_camera);

            var ents = _gameServer.EntityManager.GetEntities(Aspect.All(typeof(RenderData)));
            var viewMatrix = _camera.GetViewMatrix();
            spriteBatch.Begin(transformMatrix: viewMatrix);
            foreach(var ent in ents)
            {
                var renderData = ent.GetComponent<RenderData>();
                if (renderData != null)
                {
                    renderData.UpdateFromPhysics();
                    renderData.Draw(spriteBatch);
                }

            }
            spriteBatch.End();

            //             var midgard = _gameServer.LookupSystem<Midgard>();
            //             var world = midgard.GetWorld();
            //             var bodies = world.BodyList;
            // 
            //             spriteBatch.Begin(transformMatrix: viewMatrix);
            // 
            //             foreach (var body in bodies)
            //             {
            //                 foreach(var fix in body.FixtureList)
            //                 {
            //                     var poly = fix.Shape as PolygonShape;
            //                     Transform xf;
            //                     body.GetTransform(out xf);
            // 
            //                     Vector2[] data = new Vector2[4];
            //                     for (int i = 0; i < poly.Vertices.Count; i++)
            //                     {
            //                         Farseer.Framework.Vector2 tmp = MathUtils.Mul(ref xf, poly.Vertices[i]);
            //                         data[i] = new Vector2(tmp.X * 10f, tmp.Y * 10f);
            //                     }
            // 
            //                     DrawLine(spriteBatch, data[0], data[1]);
            //                     DrawLine(spriteBatch, data[1], data[2]);
            //                     DrawLine(spriteBatch, data[2], data[3]);
            //                     DrawLine(spriteBatch, data[3], data[0]);
            //                 }
            //             }
            //             spriteBatch.End();

            DrawStats(spriteBatch, gameTime);
            base.Draw(gameTime);
        }

        double epoc = 0f;
        NetStats stats = new NetStats();
        int clientCount = 0;
        private void DrawStats(SpriteBatch batch, GameTime gameTime)
        {
            epoc += gameTime.ElapsedGameTime.TotalSeconds;
            if (epoc >= 0.1)
            {
                epoc = 0f;
                var bifrost = _gameServer.LookupSystem<BifrostServer>();
                stats = bifrost.GetStats();

                clientCount = _gameServer.EntityManager.GetEntities(Aspect.One(typeof(PlayerComponent))).Count;
            }
            if (stats == null) return;

            var scale = Matrix.CreateScale(0.5f);
            batch.Begin(transformMatrix: scale);
            batch.DrawString(_font, "In Kbps: " + Math.Round(stats.BytesInPerSec/1024f * 8f,1), new Vector2(1320, 10), Color.Red);
            batch.DrawString(_font, "Out Kbps: " + Math.Round(stats.BytesOutPerSec/ 1024f * 8f, 1), new Vector2(1305, 40), Color.Red);
            batch.DrawString(_font, "Clients: " + clientCount, new Vector2(1320, 70), Color.Red);


            batch.End();

        }

        void DrawPoint(SpriteBatch sb, Vector2 point)
        {
            DrawLine(sb, point, point + new Vector2(1f,1f));
        }

        void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y, edge.X);


            sb.Draw(_lineTexture,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                    1), //width of line, change this to make thicker line
                null,
                Color.Red, //colour of line
                angle,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);
        }
    }
}
