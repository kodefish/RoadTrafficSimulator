using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RoadTrafficSimulator
{
    public class SpriteTest : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Texture2D carTexture;
        private float angle = 0;

        public SpriteTest()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            carTexture = Content.Load<Texture2D>("Vehicles/Car");

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            angle += 0.01f;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            DrawSprite(spriteBatch);
            spriteBatch.End();
        }

        public void DrawSprite(SpriteBatch spriteBatch)
        {
            Vector2 location = new Vector2(400, 240);
            Rectangle sourceRectangle = new Rectangle(0, 0, carTexture.Width, carTexture.Height);
            Vector2 origin = new Vector2(carTexture.Width / 2, carTexture.Height / 2);
            spriteBatch.Draw(
                carTexture, 
                location, 
                sourceRectangle, 
                Color.White, 
                angle - MathHelper.Pi, 
                origin, 
                new Vector2(1, 1), 
                SpriteEffects.None, 
                1);
        }
    }
}
