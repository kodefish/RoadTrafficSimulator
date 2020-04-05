using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.WorldEntities;
using Rectangle = RoadTrafficSimulator.Simulator.DataStructures.Geometry.Rectangle;

namespace RoadTrafficSimulator.Graphics
{
    class RTSRenderer
    {
        private RTSDatastructuresRenderer dRenderer;

        // Some color properties for the render
        private Color intersectionColor = Color.DarkGray;
        private Color roadColor = Color.Gray;

        public float Scale { get; set; }

        public RTSRenderer()
        {
            dRenderer = new RTSDatastructuresRenderer();
            Scale = 1;
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            dRenderer.LoadContent(graphicsDevice, spriteBatch);
        }

        private void DrawRectange(IRTSPosition origin, IRTSDimension dimension, Color c)
        {
            // Translate into a rectangle 
            Rectangle rect = new Rectangle(
                origin.Position * Scale,
                dimension.Dimensions.X * Scale, dimension.Dimensions.Y * Scale);

            // Draw the rectangle
            dRenderer.DrawRectangle(rect, c);
        }

        public void DrawIntersection(FourWayIntersection intersection)
        {
            DrawRectange(intersection, intersection, intersectionColor);
        }

        public void DrawRoad(Road road)
        {
            DrawRectange(road, road, roadColor);
        }

    }
}
