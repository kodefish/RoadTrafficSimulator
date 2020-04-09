using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RoadTrafficSimulator.Simulator.WorldEntities;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using Rectangle = RoadTrafficSimulator.Simulator.DataStructures.Geometry.Rectangle;
using Vector2 = RoadTrafficSimulator.Simulator.DataStructures.LinAlg.Vector2;

namespace RoadTrafficSimulator.Graphics
{
    class RTSRenderer
    {
        private RTSGeometryRenderer dRenderer;
        private SpriteBatch spriteBatch;

        // Some color properties for the render
        private Color intersectionColor = Color.DarkGray;
        private Color roadColor = Color.Gray;
        private Texture2D carTexture;

        public float Scale { get; set; }

        public RTSRenderer()
        {
            dRenderer = new RTSGeometryRenderer();
            Scale = 1;
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content)
        {
            this.spriteBatch = spriteBatch;
            dRenderer.LoadContent(graphicsDevice, spriteBatch);
            carTexture = content.Load<Texture2D>("Vehicles/Car");
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
            foreach (Lane l in road.SouthBoundLanes) DrawLane(l, Color.Pink);
            foreach (Lane l in road.NorthBoundLanes) DrawLane(l, Color.LimeGreen);
            Segment srcSegment = road.RoadStartSegment;
            Segment dstSegment = road.RoadTargetSegment;
            Vector2 sepSrc = srcSegment.GetPointOnSegment(road.NumLanesNorthBound * Lane.LANE_WIDTH / srcSegment.Length);
            Vector2 sepDst = dstSegment.GetPointOnSegment(road.NumLanesSouthBound * Lane.LANE_WIDTH/ dstSegment.Length);
            DrawSegment(new Segment(sepSrc, sepDst), Color.Yellow);
        }

        public void DrawLane(Lane l, Color c)
        {
            // Draw the midline
            // DrawPath(l.Path, Color.Red);

            // Draw the vector to the closest corner of car in front
            List<Car> cars = l.Cars;
            for (int i = 0; i < l.Cars.Count - 1; i++)
            {
                Car c1 = cars[i]; Car c2 = cars[i + 1];
                Vector2 p1 = c2.GetGeometricalFigure().ClosestVertex(c1.Position);
                Vector2 p2 = c1.GetGeometricalFigure().ClosestVertex(p1);
                dRenderer.Draw(new Segment(p1 * Scale, p2 * Scale), Color.Cyan);
            }
        }

        public void DrawCar(Car c, Color color)
        {
            Vector2 location = c.Position * Scale;
            Rectangle sourceRectangle = new Rectangle(new Vector2(0, 0), c.CarWidth, c.CarLength);
            Vector2 origin = new Vector2(c.CarWidth / 2, c.CarLength / 2);
            Vector2 scale = new Vector2(Scale, Scale);

            dRenderer.Draw(sourceRectangle, color, location, origin, scale, c.Angle);
        }
    }
}
