using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoadTrafficSimulator.Graphics;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using Vector2 = RoadTrafficSimulator.Simulator.DataStructures.LinAlg.Vector2;

namespace RoadTrafficSimulator
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class BezierPlayground : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        float displayWidth, displayHeight;

        // Renderer for bezier curve
        RTSGeometryRenderer rtsRendrer;

        // Bezier curve params
        float centerHorL, centerHorR, centerVertL, centerVertR;
        float spacingVertL, spacingVertR;
        float step;


        public BezierPlayground()
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

            rtsRendrer = new RTSGeometryRenderer();

            displayWidth = GraphicsDevice.DisplayMode.Width;
            displayHeight = GraphicsDevice.DisplayMode.Height;

            centerHorR = 0.25f * displayWidth;
            centerHorL = 0.75f * displayWidth;
            centerVertL = 0.5f * displayHeight;
            centerVertR = 0.5f * displayHeight;
            spacingVertL =  0.25f * displayHeight;
            spacingVertR = 0.25f * displayHeight;

            step = displayHeight / 100;

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

            KeyboardState kbState = Keyboard.GetState();
            if (kbState.IsKeyDown(Keys.W))
            {
                spacingVertR = Math.Min(spacingVertR + step, 0.5f * displayHeight);
            }
            if (kbState.IsKeyDown(Keys.S))
            {
                spacingVertR = Math.Max(spacingVertR - step, 0);
            }
            if (kbState.IsKeyDown(Keys.Up))
            {
                spacingVertL = Math.Min(spacingVertL + step, 0.5f * displayHeight);
            }
            if (kbState.IsKeyDown(Keys.Down))
            {
                spacingVertL = Math.Max(spacingVertL - step, 0);
            }

            if (kbState.IsKeyDown(Keys.D))
            {
                centerHorR = Math.Min(centerHorR + step, 0.5f * displayWidth);
            }
            if (kbState.IsKeyDown(Keys.A))
            {
                centerHorR = Math.Max(centerHorR - step, 0);
            }
            if (kbState.IsKeyDown(Keys.Right))
            {
                centerHorL = Math.Min(centerHorL + step, displayWidth);
            }
            if (kbState.IsKeyDown(Keys.Left))
            {
                centerHorL = Math.Max(centerHorL - step, 0.5f * displayWidth);
            }

            if (kbState.IsKeyDown(Keys.Q))
            {
                centerVertR = Math.Min(centerVertR - step, displayWidth);
            }
            if (kbState.IsKeyDown(Keys.E))
            {
                centerVertR = Math.Max(centerVertR + step, 0);
            }
            if (kbState.IsKeyDown(Keys.PageUp))
            {
                centerVertL = Math.Min(centerVertL - step, displayHeight);
            }
            if (kbState.IsKeyDown(Keys.PageDown))
            {
                centerVertL = Math.Max(centerVertL + step, 0);
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

            spriteBatch.Begin();

            DrawRTSDatastructures();

            spriteBatch.End();
        }

        private void DrawRTSDatastructures()
        {
            Vector2 a1 = new Vector2(centerHorL, centerVertL + spacingVertL);
            Vector2 c1 = new Vector2(centerHorL, centerVertL - spacingVertL);
            Vector2 a2 = new Vector2(centerHorR, centerVertR - spacingVertR);
            Vector2 c2 = new Vector2(centerHorR, centerVertR + spacingVertR);
            BezierCurve bCurve = new BezierCurve(a1, c1, a2, c2);

            rtsRendrer.DrawPoint(a1, Color.Red, 10);
            rtsRendrer.DrawPoint(c1, Color.Green, 10);
            rtsRendrer.DrawPoint(a2, Color.Red, 10);
            rtsRendrer.DrawPoint(c2, Color.Green, 10);
            rtsRendrer.Draw(bCurve, Color.White, 2, 1 / 1000f, true);
        }
    }
}
