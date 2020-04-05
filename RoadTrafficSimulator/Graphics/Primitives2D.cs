using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoadTrafficSimulator.Graphics
{
    /// <summary>
    /// Basic C# code to render pixels, lines and rectangles in 2D
    /// adaptded from: Source: https://www.dreamincode.net/forums/topic/234298-c%23-xna-2d-primitives-pixel-line-rectangle-etc/
    /// </summary>
    class Primitives2D
    {
        Texture2D pixelTexture;         // 1x1 white texture, think of it as a paint brush
        GraphicsDevice graphicsDevice;  // Used to draw on the screen
        SpriteBatch spriteBatch;        // Spritebatch used

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;

            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });
        }

        // Draws a pixel of color col at (x, y)
        public void DrawPixel(float x, float y, Color color, float thickness = 1)
        {
            int adjustedX = (int) (x - (thickness / 2));
            int adjustedY = (int) (y - (thickness / 2));
            spriteBatch.Draw(pixelTexture, new Rectangle(adjustedX, adjustedY, (int) thickness, (int) thickness), color);
        }

        // Draw line from (x1, y1) to (x2, y2)
        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1)
        {
            // Get actualy points
            Vector2 p1 = new Vector2(x1, y1);
            Vector2 p2 = new Vector2(x2, y2);

            // Comput length and angle
            float length = Vector2.Distance(p1, p2);
            float angle = (float) Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            // Construct and draw corresponding line
            Rectangle rectangle = new Rectangle((int) x1, (int) y1, (int) length, (int) thickness);
            spriteBatch.Draw(pixelTexture, rectangle, null, color, angle, Vector2.Zero, SpriteEffects.None, 0.0f);
        }

        // Draw rectangle centered at (x, y), of of size widthxheight, of a certain color
        public void DrawRectangle(float x, float y, float width, float height, Color color, bool filled = true)
        {
            if (filled)
            {
                spriteBatch.Draw(pixelTexture, new Rectangle((int) x, (int) y, (int) width, (int) height), color);
            }
            else
            {
                DrawLine(x, y, x + width, y, color);                    // Top
                DrawLine(x + width, y, x + width, y + height, color);   // Right
                DrawLine(x + width, y + height, x, y + height, color);  // Bottom
                DrawLine(x, y + height, x, y, color);           // Left
            }
        }
    }
}
