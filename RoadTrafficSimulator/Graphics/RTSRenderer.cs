using System.Diagnostics;
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
        private Color[] colors = { Color.Red, Color.Green, Color.Blue };

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
            Vector2 location = rectangle.Origin * Scale;
            Rectangle sourceRectangle = new Rectangle(new Vector2(0, 0), rectangle.Width, rectangle.Length);
            Vector2 origin = new Vector2(rectangle.Width / 2, rectangle.Length / 2);
            Vector2 scale = new Vector2(Scale, Scale);

            dRenderer.Draw(sourceRectangle, c, location, origin, scale, rectangle.Angle);
        }

        private void DrawSegment(Segment s, Color c, float thickness = 1)
        {
            Segment scaledSegment = new Segment(s.Source * Scale, s.Target * Scale);
            dRenderer.Draw(scaledSegment, c, thickness);
        }

        public void DrawPath(Path p, Color c)
        {
            foreach (Segment s in p.Segments) DrawSegment(s, c);
        }


        public void DrawIntersection(FourWayIntersection intersection)
        {
            DrawRectange(intersection.GetGeometricalFigure(), intersectionColor);
        }

        public void DrawNormal(Segment s, Color c, float thickness = 1)
        {
            Segment normalVec = new Segment(
                s.Midpoint, 
                s.Midpoint + s.Vector.Normal);

            DrawSegment(normalVec, c, thickness);
        }

        public void DrawRoad(Road road)
        {
            DrawRectange(road.GetGeometricalFigure(), roadColor);
            foreach (Lane l in road.InLanes) DrawLane(l, colors[l.LaneIdx % colors.Length]);
            foreach (Lane l in road.OutLanes) DrawLane(l, colors[l.LaneIdx % colors.Length]);
            DrawSegment(road.RoadMidline, Color.Yellow);

            // Draw the source and target segment normals
            DrawNormal(road.RoadStartSegment, Color.Pink, 2);
            DrawNormal(road.RoadTargetSegment, Color.Purple, 2);
        }

        public void DrawLane(Lane l, Color c)
        {
            // Draw the midline
            foreach (Segment s in l.Path.Segments) DrawSegment(s, c, 5);

            // Draw arrival points
            dRenderer.DrawPoint(l.Path.PathStart * Scale, Color.Green, 5);
            dRenderer.DrawPoint(l.Path.PathEnd * Scale, Color.Red, 5);
            foreach (Segment s in l.Path.Segments) dRenderer.Draw(new Segment(s.Source * Scale, s.Target * Scale), Color.LightGray);

            DrawNormal(l.SourceSegment, Color.MonoGameOrange, 3);
            DrawNormal(l.TargetSegment, Color.MonoGameOrange, 3);
            /*
            // Draw free space
            Segment freeLaneSpace = new Segment(
                l.Path.PathStart * Scale, 
                (l.Path.PathStart + l.Path.TangentOfProjectedPosition(l.Path.PathStart) * l.FreeLaneSpace()) * Scale);
            dRenderer.Draw(freeLaneSpace, Color.Pink);

            // Draw the vector to the closest corner of car in front
            List<Car> cars = l.Cars;
            for (int i = 0; i < l.Cars.Count - 1; i++)
            {
                Car c1 = cars[i]; Car c2 = cars[i + 1];
                Vector2 p1 = c2.GetGeometricalFigure().ClosestVertex(c1.Position);
                Vector2 p2 = c1.GetGeometricalFigure().ClosestVertex(p1);
                dRenderer.Draw(new Segment(p1 * Scale, p2 * Scale), Color.Cyan);
            }
            */
        }

        public void DrawCar(Car c, Color color)
        {
            DrawRectange(c.GetGeometricalFigure(), color);
        }
    }
}
