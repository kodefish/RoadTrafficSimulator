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

        private void DrawRectange(Rectangle rectangle, Color c)
        {
            // Translate into a rectangle 
            Rectangle rect = new Rectangle(
                rectangle.Origin * Scale,
                rectangle.Width * Scale, rectangle.Length * Scale);

            // Draw the rectangle
            dRenderer.Draw(rect, c);
        }

        private void DrawSegment(Segment s, Color c)
        {
            Segment scaledSegment = new Segment(s.Source * Scale, s.Target * Scale);
            dRenderer.Draw(scaledSegment, c);
        }

        public void DrawPath(Path p, Color c)
        {
            foreach (Segment s in p.Segments) DrawSegment(s, c);
        }


        public void DrawIntersection(FourWayIntersection intersection)
        {
            DrawRectange(intersection.GetGeometricalFigure(), intersectionColor);
        }

        public void DrawRoad(Road road)
        {
            DrawRectange(road.GetGeometricalFigure(), roadColor);
            foreach (Lane l in road.SouthBoundLanes) DrawPath(l.Path, Color.Red);
            foreach (Lane l in road.NorthBoundLanes) DrawPath(l.Path, Color.Green);
            Segment srcSegment = road.RoadStartSegment;
            Segment dstSegment = road.RoadTargetSegment;
            Vector2 sepSrc = srcSegment.GetPointOnSegment(road.NumLanesNorthBound * Lane.LANE_WIDTH / srcSegment.Length);
            Vector2 sepDst = dstSegment.GetPointOnSegment(road.NumLanesSouthBound * Lane.LANE_WIDTH/ dstSegment.Length);
            DrawSegment(new Segment(sepSrc, sepDst), Color.Yellow);
        }

        public void DrawCar(Car c)
        {
            Rectangle r = new Rectangle(c.Position * Scale, Scale, Scale);
            dRenderer.Draw(r, Color.Blue);
        }
    }
}
