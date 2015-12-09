using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System;
using Artemis;
using Shared;
using MonoGame.Extended;
using Asgard;
using Asgard.Core.Network.Packets;
using Asgard.EntitySystems.Components;
using Asgard.Core.Network;
using MonoGame.Extended.BitmapFonts;

namespace Mono_Client
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        gameClient _gameClient;
        Entity _mapEntity;
        Camera2D _camera;
        float _worldSpace = 1f / 10f;
        BitmapFont _font;

        public Game1()
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

            _camera = new Camera2D(GraphicsDevice);
            _camera.Zoom = 4.5f;
            _camera.Position = new Vector2(-120, -5);


            IsMouseVisible = true;
            base.Initialize();

            _gameClient = new gameClient();
            _gameClient.Content = Content;
            RenderData.Content = Content;
            var th = new Thread(() =>
            {
                _gameClient.Run();
            });

            th.IsBackground = true;
            th.Start();

            BuildWorld();
        }

        private void BuildWorld()
        {
            _mapEntity = _gameClient.EntityManager.Create(1);
            var mapComponent = new MapComponent();
            mapComponent.Device = GraphicsDevice;
            mapComponent.Texture = Content.Load<Texture2D>("roguelikeSheet_transparent");
            _mapEntity.AddComponent(mapComponent);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            var renderData = _gameClient.GetPlayerData();
            if (renderData != null)
            {
                _camera.LookAt(renderData.GetPosition());
            }


            // TODO: Add your update logic here
            var mState = Mouse.GetState();
            if (mState.LeftButton == ButtonState.Pressed)
            {
                var screenPos = mState.Position.ToVector2();

                if (!Window.ClientBounds.Contains(mState.Position))
                    return;

                var worldPos = _camera.ScreenToWorld(screenPos);
                worldPos *= _worldSpace;
                _gameClient.CurrentState.LeftMouseDown = true;
                _gameClient.CurrentState.MousePositionInWorld = new Farseer.Framework.Vector2(worldPos.X, worldPos.Y);
            }
            else
            {
                _gameClient.CurrentState.LeftMouseDown = false;

                bool isMovingTo = (renderData != null && renderData.MovingToPosition);
                if (!isMovingTo)
                    _gameClient.CurrentState.MousePositionInWorld = Farseer.Framework.Vector2.Zero;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var viewmap = _mapEntity.GetComponent<MapComponent>().Map;
            if (viewmap != null)
            {
                try
                {
                    viewmap.Draw(_camera);
                }
                catch
                {

                }
            }

            var ents = _gameClient.EntityManager.GetEntities(Aspect.One(typeof(RenderData)));
            spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
            foreach(var ent in ents)
            {
                var rd = ent.GetComponent<RenderData>();
                rd.UpdateFromPhysics();
                rd.Draw(spriteBatch);
            }
            spriteBatch.End();

            DrawStats(spriteBatch, gameTime);
            base.Draw(gameTime);
        }

        double epoc = 0f;
        NetStats stats = new NetStats();
        private void DrawStats(SpriteBatch batch, GameTime gameTime)
        {
            epoc += gameTime.ElapsedGameTime.TotalSeconds;
            if (epoc >= 0.1)
            {
                epoc = 0f;
                var bifrost = _gameClient.LookupSystem<BifrostClient>();
                stats = bifrost.GetStats();
            }

            if (stats == null) return;

            var scale = Matrix.CreateScale(0.5f);
            batch.Begin(transformMatrix: scale);
            batch.DrawString(_font, "In Kbps: " + Math.Round(stats.BytesInPerSec / 1024f * 8f, 1), new Vector2(1320, 10), Color.Red);
            batch.DrawString(_font, "Out Kbps: " + Math.Round(stats.BytesOutPerSec / 1024f * 8f, 1), new Vector2(1305, 40), Color.Red);
            batch.DrawString(_font, "Ping: " + Math.Round(stats.AvgPing,2) + "ms", new Vector2(1370, 70), Color.Red);


            batch.End();

        }
    }
}
