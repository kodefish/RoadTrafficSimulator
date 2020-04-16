using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RoadTrafficSimulator.Simulator.WorldEntities;
using RoadTrafficSimulator.Simulator.IntersectionLogic;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;
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

        public void DrawScaledRectange(Rectangle rectangle, Color c)
        {
            Vector2 location = rectangle.Origin * Scale;
            Rectangle sourceRectangle = new Rectangle(new Vector2(0, 0), rectangle.Width, rectangle.Length);
            Vector2 origin = new Vector2(rectangle.Width / 2, rectangle.Length / 2);
            Vector2 scale = new Vector2(Scale, Scale);

            dRenderer.Draw(sourceRectangle, c, location, origin, scale, rectangle.Angle);
        }

        public void DrawScaledSegment(Segment s, Color c, float thickness = 1)
        {
            Segment scaledSegment = new Segment(s.Source * Scale, s.Target * Scale);
            dRenderer.Draw(scaledSegment, c, thickness);
        }

        public void DrawIntersection(FourWayIntersection intersection)
        {
            DrawScaledRectange(intersection.GetGeometricalFigure(), intersectionColor);
            foreach (List<Lane> ll in intersection.CurrentTrafficLightState.activeLanes.Values)
            {
                foreach (Lane l in ll) DrawLane(l, Color.Pink);
            }
        }

        public void DrawRoad(Road road)
        {
            DrawScaledRectange(road.GetGeometricalFigure(), roadColor);
            foreach (Lane l in road.InLanes) DrawLane(l, colors[l.LaneIdx % colors.Length]);
            foreach (Lane l in road.OutLanes) DrawLane(l, colors[l.LaneIdx % colors.Length]);
            DrawScaledSegment(road.RoadMidline, Color.Yellow);

            // Draw lane normals
            /*
            DrawScaledSegment(new Segment(road.InLanesSourceSegment.Midpoint, road.InLanesSourceSegment.Midpoint + road.InLanesSourceSegment.Direction.Normal), Color.Honeydew);
            DrawScaledSegment(new Segment(road.InLanesTargetSegment.Midpoint, road.InLanesTargetSegment.Midpoint + road.InLanesTargetSegment.Direction.Normal), Color.Honeydew);
            DrawScaledSegment(new Segment(road.OutLanesSourceSegment.Midpoint, road.OutLanesSourceSegment.Midpoint + road.OutLanesSourceSegment.Direction.Normal), Color.Honeydew);
            DrawScaledSegment(new Segment(road.OutLanesTargetSegment.Midpoint, road.OutLanesTargetSegment.Midpoint + road.OutLanesTargetSegment.Direction.Normal), Color.Honeydew);
            */
        }

        public void DrawLane(Lane l, Color c)
        {
            // Draw arrival points
            dRenderer.DrawPoint(l.Path.PathStart * Scale, Color.Green, 5);
            dRenderer.DrawPoint(l.Path.PathEnd * Scale, Color.Red, 5);
            foreach (Segment s in l.Path.Segments) DrawScaledSegment(s, c);
    }

        public void DrawCar(Car c)
        {
            Color color;
            if (c.DrivingState is KeepLaneState) color = Color.Green;
            else if (c.DrivingState is ChangeLaneState) color = Color.Yellow;
            else /*if (c.DrivingState is WaitingForLightState)*/ color = Color.Red;
            DrawScaledRectange(c.GetGeometricalFigure(), color);
            Segment carVel = new Segment(c.Position, c.Position + c.LinearVelocity);
            DrawScaledSegment(carVel, Color.Yellow, 2);

        }
    }
}
