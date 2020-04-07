using System;
namespace RoadTrafficSimulator.Simulator.DataStructures.LinAlg
{
    class Vector2
    {
        // Vector constants
        public static Vector2 Up => new Vector2(0, 1);
        public static Vector2 Down => new Vector2(0, -1);
        public static Vector2 Left => new Vector2(1, 0);
        public static Vector2 Right => new Vector2(-1, 0);

        public float X { get; }
        public float Y { get; }

        public Vector2() => new Vector2(0, 0);

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Vector properties
        public float Length => (float)Math.Sqrt(X * X + Y * Y);
        public Vector2 Normalized => new Vector2(X / Length, Y / Length);
        public Vector2 Normal => new Vector2(-Y, X);

        // Operator on single vector
        public static Vector2 operator +(Vector2 a) => a;
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);

        // Vector addition, subtraction, scalar multiplication and scalar division
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float c) => new Vector2(a.X * c, a.Y * c);
        public static Vector2 operator *(float c, Vector2 a) => new Vector2(a.X * c, a.Y * c);
        public static Vector2 operator /(Vector2 a, float c) => new Vector2(a.X / c, a.Y / c);
        public static Vector2 operator /(float c, Vector2 a) => new Vector2(a.X / c, a.Y / c);

        // Vector functions
        public static float SignedDistance(Vector2 a, Vector2 b) => (b - a).Length;
        public static float Distance(Vector2 a, Vector2 b) => Math.Abs((b - a).Length);
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {
                Vector2 other = (Vector2)obj;
                return other.X == X && other.Y == Y;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }
    }

}
