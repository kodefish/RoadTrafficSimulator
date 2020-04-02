using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoadTrafficSimulator.GraphicPrimitives;

namespace RoadTrafficSimulator
{
    // Class that draws RTS datastructures using XNA's framework
    class DrawRTSDatastructures
    {
        private readonly Primitives2D primitives2D;
        public DrawRTSDatastructures(){
            primitives2D = new Primitives2D();
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            primitives2D.LoadContent(graphicsDevice, spriteBatch);
        }

        public void DrawPoint(DataStructures.Vector2 point, Color c)
        {
            primitives2D.DrawPixel(point.X, point.Y, c);
        }

        public void DrawSegment(DataStructures.Segment segment, Color c)
        {
            primitives2D.DrawLine(
                segment.Source.X, segment.Source.Y,
                segment.Target.X, segment.Target.Y,
                c);
        }
        
        public void DrawRectangle(DataStructures.Rectangle rect, Color c, bool filled = true)
        {
            if (filled)
            {
                float x = rect.TopLeft.X;
                float y = rect.TopLeft.Y;
                primitives2D.DrawRectangle(x, y, rect.Width, rect.Length, c, filled);
            }
            foreach (DataStructures.Segment side in rect.Sides)
            {
                DrawSegment(side, c);
            }
        }

        public void DrawBezierCurve(DataStructures.BezierCurve bCurve, Color c)
        {
            throw new NotImplementedException();
        }

    }
}
