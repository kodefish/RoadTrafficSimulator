using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    /// <summary>
    /// Rectangle centered about the origin 
    /// </summary>
    class Rectangle : GeometricalFigure
    {
        public Vector2 Origin { get; }
        public float Width { get; }
        public float Length { get; }

        public Rectangle(Vector2 origin, float width, float length)
        {
            Origin = origin;
            Width = width;
            Length = length;
        }

        public Vector2 TopLeft { get => Origin      + new Vector2(-Width / 2, -Length / 2); }
        public Vector2 TopRight { get => Origin     + new Vector2( Width / 2, -Length / 2); }
        public Vector2 BottomRight { get => Origin  + new Vector2( Width / 2,  Length / 2); }
        public Vector2 BottomLeft { get => Origin   + new Vector2(-Width / 2,  Length / 2); }

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

        public Vector2 ClosestVertex(Vector2 position)
        {
            // Check which vertex is closer to position
            float minDist = float.PositiveInfinity;  Vector2 closestVertex = null;
            foreach(Vector2 vertex in Vertices)
            {
                float dist = Vector2.Distance(position, vertex);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestVertex = vertex;
                }
            }

            return closestVertex;
        }

        public override string ToString()
        {
            return String.Format("Rectangle: origin {0}, width {1}, length {2}", Origin, Width, Length);
        }

    }
}
