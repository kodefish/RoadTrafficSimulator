﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using Vector2 = RoadTrafficSimulator.Simulator.DataStructures.LinAlg.Vector2;
using Rectangle = RoadTrafficSimulator.Simulator.DataStructures.Geometry.Rectangle;

namespace RoadTrafficSimulator.Graphics
{
    // Class that draws RTS datastructures using XNA's framework
    class RTSGeometryRenderer
    {
        private readonly Primitives2D primitives2D;
        public RTSGeometryRenderer(){
            primitives2D = new Primitives2D();
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            primitives2D.LoadContent(graphicsDevice, spriteBatch);
        }

        public void Draw(Vector2 point, Color c, float thickness = 1)
        {
            primitives2D.DrawPixel(point.X, point.Y, c, thickness);
        }

        public void Draw(Segment segment, Color c, float thickness = 1)
        {
            primitives2D.DrawLine(
                segment.Source.X, segment.Source.Y,
                segment.Target.X, segment.Target.Y,
                c, thickness);
        }
        
        public void Draw(Rectangle rect, Color c, bool filled = true)
        {
            if (filled)
            {
                float x = rect.TopLeft.X;
                float y = rect.TopLeft.Y;
                primitives2D.DrawRectangle(x, y, rect.Width, rect.Length, c, filled);
            }
            foreach (Segment side in rect.Sides)
            {
                Draw(side, c);
            }
        }

        public void Draw(BezierCurve bCurve, Color c, float thickness, float step = 0.01f, bool drawTangent = false)
        {
            for (float t = 0; t <= 1; t += step)
            {
                Vector2 point = bCurve.GetPosition(t);
                Draw(point, c, thickness);

                if (drawTangent)
                {
                    Vector2 tangent = bCurve.GetTangent(t);
                    Segment tangentSegment = new Segment(point, point + tangent.Normalized * 500);
                    Draw(tangentSegment, Color.Red, thickness * 0.5f);
                }
            }
        }

    }
}
