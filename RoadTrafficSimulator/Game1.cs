using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoadTrafficSimulator.Graphics;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using Vector2 = RoadTrafficSimulator.Simulator.DataStructures.LinAlg.Vector2;
using Rectangle = RoadTrafficSimulator.Simulator.DataStructures.Geometry.Rectangle;

namespace RoadTrafficSimulator
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Graphics primitives
        Primitives2D primitives2D;
        RTSGeometryRenderer rtsRendrer;
        
        
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
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            // TODO: Add your initialization logic here
            primitives2D = new Primitives2D();
            rtsRendrer = new RTSGeometryRenderer();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load content for primitive drawer
            primitives2D.LoadContent(GraphicsDevice, spriteBatch);
            rtsRendrer.LoadContent(GraphicsDevice, spriteBatch);
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

            spriteBatch.Begin();

            // Draw2dPrimitives();
            DrawRTSDatastructures();

            spriteBatch.End();
        }

        private void DrawRTSDatastructures()
        {

            float displayWidth = GraphicsDevice.DisplayMode.Width;
            float displayHeight = GraphicsDevice.DisplayMode.Height;

            Vector2 origin = new Vector2(displayWidth / 2, displayHeight / 2);
            Rectangle rectangle = new Rectangle(
                origin,
                displayWidth / 2, displayHeight / 2
            );

            rtsRendrer.Draw(rectangle, Color.Red, false);

            Vector2 p1 = new Vector2(displayWidth * .25f, displayHeight * 0.75f);
            Vector2 p2 = new Vector2(displayWidth * .25f, displayHeight * 0.25f);
            Vector2 p3 = new Vector2(displayWidth * .75f, displayHeight * 0.75f);
            Vector2 p4 = new Vector2(displayWidth * .75f, displayHeight * 0.25f);
            BezierCurve bCurve = new BezierCurve(p1, p2, p3, p4);

            rtsRendrer.DrawPoint(p1, Color.Red, 10);
            rtsRendrer.DrawPoint(p2, Color.Green, 10);
            rtsRendrer.DrawPoint(p3, Color.Blue, 10);
            rtsRendrer.DrawPoint(p4, Color.Purple, 10);
            rtsRendrer.Draw(bCurve, Color.White, 2, 1/1000f);
        }

        private void Draw2dPrimitives()
        {
            float displayWidth = GraphicsDevice.DisplayMode.Width;
            float displayHeight = GraphicsDevice.DisplayMode.Height;
            primitives2D.DrawRectangle(
                displayWidth / 4, displayHeight / 4,
                displayWidth / 2, displayHeight / 2,
                Color.Red, false);

            primitives2D.DrawLine(
                displayWidth / 4, displayHeight / 2,
                3 * displayWidth / 4, displayHeight / 2,
                Color.Green, 5);
        }
    }
}
