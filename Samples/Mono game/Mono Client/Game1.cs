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
using Mono_Server;
using Asgard.EntitySystems.Components;

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
            _camera.Zoom = 2.0f;
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

            PacketFactory.AddCallback<LoginResponsePacket>(onLogin);

            var bifrost = _gameClient.LookupSystem<BifrostClient>();
            bifrost.OnConnection += Bifrost_OnConnection;
            bifrost.Start();

        }

        private void Bifrost_OnConnection(Asgard.Core.Network.NetNode connection)
        {
            var bifrost = _gameClient.LookupSystem<BifrostClient>();
            MonoLoginPacket packet = new MonoLoginPacket();
            bifrost.Send(packet);
        }

        private void onLogin(LoginResponsePacket obj)
        {

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

            // TODO: Add your update logic here

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
                var pc = ent.GetComponent<Physics2dComponent>();
                rd.UpdateFromPhysics(pc);
                rd.Draw(spriteBatch);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
