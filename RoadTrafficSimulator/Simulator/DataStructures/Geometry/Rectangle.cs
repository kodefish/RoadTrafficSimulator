using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    /// <summary>
    /// Rectangle centered about the origin 
    /// </summary>
    class Rectangle : GeometricalFigure
    {
        /// <summary>
        /// Center of the rectangle
        /// </summary>
        public Vector2 Origin { get; }

        /// <summary>
        /// Width of the rectangle, along the horizontal axis in local coordinates
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Height/Length of the rectangle, along the vertical axis in local coordinates
        /// </summary>
        public float Length { get; }

        /// <summary>
        /// Angle between global horizontal and horizontal X-axis, in radians
        /// </summary>
        public float Angle { get; private set; }

        /// <summary>
        /// Constructs a rectangle
        /// </summary>
        /// <param name="origin">Center of the rectangle</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="length">Length of the rectangle</param>
        /// <param name="angle">Rotation, in radians</param>
        public Rectangle(Vector2 origin, float width, float length, float angle = 0)
        {
            Origin = origin;
            Width = width;
            Length = length;
            Angle = angle;
        }

        /// <summary>
        /// Top left corner, in local coordinates
        /// </summary>
        public Vector2 TopLeft { get => Origin      + Vector2.Rotate(new Vector2(-Width / 2, -Length / 2), Angle); }

        /// <summary>
        /// Top right corner, in local coordinates
        /// </summary>
        public Vector2 TopRight { get => Origin     + Vector2.Rotate(new Vector2( Width / 2, -Length / 2), Angle); }

        /// <summary>
        /// Bottom right corner, in local coordinates
        /// </summary>
        public Vector2 BottomRight { get => Origin  + Vector2.Rotate(new Vector2( Width / 2,  Length / 2), Angle); }

        /// <summary>
        /// Bottom left corner, in local coordinates
        /// </summary>
        public Vector2 BottomLeft { get => Origin   + Vector2.Rotate(new Vector2(-Width / 2,  Length / 2), Angle); }

        /// <summary>
        /// Array of all four vertices, as follows: top left, top right, bottom right, bottom left
        /// </summary>
        public Vector2[] Vertices
        {
            get => new Vector2[]{
                TopLeft, TopRight, BottomRight, BottomLeft
            };
        }

        /// <summary>
        /// Check for containement
        /// </summary>
        /// <param name="pt">Point to check</param>
        /// <returns>True if point in the rectangle</returns>
        public bool ContainsPoint(Vector2 pt)
        {
            Vector2 min = new Vector2(), max = new Vector2();
            float minDist = float.PositiveInfinity, maxDist = float.NegativeInfinity;
            foreach(Vector2 corner in Vertices)
            {
                float distToOrigin = corner.Norm;
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

        /// <summary>
        /// Top segment of the rectangle, normal facing away from the center
        /// </summary>
        public Segment Top { get => new Segment(TopLeft, TopRight); }

        /// <summary>
        /// Right segment of the rectangle, normal facing away from the center
        /// </summary>
        public Segment Right { get => new Segment(TopRight, BottomRight); }

        /// <summary>
        /// Bottom segment of the rectangle, normal facing away from the center
        /// </summary>
        public Segment Bottom { get => new Segment(BottomRight, BottomLeft); }

        /// <summary>
        /// Left segment of the rectangle, normal facing away from the center
        /// </summary>
        public Segment Left { get => new Segment(BottomLeft, TopLeft); }

        /// <summary>
        /// Sides of the rectangle, all normals facing out. 
        /// In order: top, left, bottom, right
        /// </summary>
        public Segment[] Sides { get => new Segment[] { Top, Left, Bottom, Right }; }

        /// <summary>
        /// Get a side of the rectangle.
        /// 0: Top
        /// 1: Left
        /// 2: Bottom
        /// 3: Right
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Segment GetSide(int i)
        {
            if (i > 3) throw new ArgumentOutOfRangeException(String.Format("{0} is out of range (0-3)", i));
            return Sides[i];
        }

        /// <summary>
        /// Returns vertex closest to given point
        /// </summary>
        /// <param name="position">Point to find closest vertex of</param>
        /// <returns>Vertex closest to given point</returns>
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
            return String.Format("Rectangle: origin {0}, width {1}, length {2}, angle {3}", Origin, Width, Length, Angle);
        }

    }
}
