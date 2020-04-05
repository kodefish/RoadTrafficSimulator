using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.WorldEntities;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using Rectangle = RoadTrafficSimulator.Simulator.DataStructures.Geometry.Rectangle;
using Vector2 = RoadTrafficSimulator.Simulator.DataStructures.LinAlg.Vector2;

namespace RoadTrafficSimulator.Graphics
{
    class RTSRenderer
    {
        private RTSGeometryRenderer dRenderer;

        // Some color properties for the render
        private Color intersectionColor = Color.DarkGray;
        private Color roadColor = Color.Gray;

        public float Scale { get; set; }

        public RTSRenderer()
        {
            dRenderer = new RTSGeometryRenderer();
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

        private void DrawSegment(Segment s, Color c)
        {
            Segment scaledSegment = new Segment(s.Source * Scale, s.Target * Scale);
            dRenderer.DrawSegment(scaledSegment, c);
        }

        public void DrawIntersection(FourWayIntersection intersection)
        {
            DrawRectange(intersection, intersection, intersectionColor);
        }

        public void DrawRoad(Road road)
        {
            DrawRectange(road, road, roadColor);
            foreach (Lane l in road.SouthBoundLanes) DrawSegment(l.Midline, Color.Red);
            foreach (Lane l in road.NorthBoundLanes) DrawSegment(l.Midline, Color.Green);
            Segment srcSegment = road.RoadStartSegment;
            Segment dstSegment = road.RoadTargetSegment;
            Vector2 sepSrc = srcSegment.GetPointOnSegment(road.NumLanesNorthBound * Lane.LANE_WIDTH / srcSegment.Length);
            Vector2 sepDst = dstSegment.GetPointOnSegment(road.NumLanesSouthBound * Lane.LANE_WIDTH/ dstSegment.Length);
            DrawSegment(new Segment(sepSrc, sepDst), Color.Yellow);
        }

        public void DrawCar(Car c)
        {
            Rectangle r = new Rectangle(c.Positon * Scale, Scale, Scale);
            dRenderer.DrawRectangle(r, Color.Blue);
        }
    }
}
