using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadTrafficSimulator.DataStructures
{
    /// <summary>
    /// Rectangle centered about the origin 
    /// </summary>
    class Rectangle
    {
        private readonly Vector2 origin;
        private readonly float width, length;

        public Rectangle(Vector2 origin, float width, float length)
        {
            this.origin = origin;
            this.width = width;
            this.length = length;
        }

        public Vector2 TopLeft { get => origin      + new Vector2(-width / 2, -length / 2); }
        public Vector2 TopRight { get => origin     + new Vector2( width / 2, -length / 2); }
        public Vector2 BottomRight { get => origin  + new Vector2( width / 2,  length / 2); }
        public Vector2 BottomLeft { get => origin   + new Vector2(-width / 2,  length / 2); }

        public float Width { get => width; }
        public float Length { get => length; }

        public Vector2[] Vertices
        {
            get => new Vector2[]{
                TopLeft, TopRight, BottomRight, BottomLeft
            };
        }

        public bool ContainsPoint(Vector2 pt)
        {
            Vector2 min = new Vector2(), max = new Vector2();
            float minDist = float.PositiveInfinity, maxDist = float.NegativeInfinity;
            foreach(Vector2 corner in Vertices)
            {
                float distToOrigin = corner.Length;
                if (distToOrigin < minDist)
                {
                    minDist = distToOrigin;
                    min = corner;
                }
                if (distToOrigin > maxDist)
                {
                    maxDist = distToOrigin;
                    max = corner;
                }
            }
            return min.X <= pt.X && pt.X <= max.X &&
                min.Y <= pt.Y && pt.Y <= max.Y;
        }

        public Segment Top { get => new Segment(TopRight, TopLeft); }
        public Segment Right { get => new Segment(BottomRight, TopRight); }
        public Segment Bottom { get => new Segment(BottomLeft, BottomRight); }
        public Segment Left { get => new Segment(TopLeft, BottomLeft); }

        public Segment[] Sides { get => new Segment[] { Top, Left, Bottom, Right }; }

        public Segment GetSide(int i)
        {
            if (i > 3) throw new ArgumentOutOfRangeException(String.Format("{0} is out of range (0-3)", i));
            return Sides[i];
        }

        public override string ToString()
        {
            return String.Format("Rectangle: origin {0}, width {1}, length {2}", origin, width, length);
        }

    }
}
