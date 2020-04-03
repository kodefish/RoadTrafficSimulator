using System;
namespace RoadTrafficSimulator.DataStructures
{
    class Vector2
    {
        private readonly float x, y;

        public Vector2() => new Vector2(0, 0);

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float X { get => x; }
        public float Y { get => y; }

        public float Length
        {
            get { return (float) Math.Sqrt(x * x + y * y); }
        }

        public Vector2 Normalized
        {
            get { return new Vector2(x / Length, y / Length); }
        }

        public static Vector2 operator +(Vector2 a) => a;
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator *(Vector2 a, float c)
        {
            return new Vector2(a.x * c, a.y * c);
        }

        public static Vector2 operator /(Vector2 a, float c)
        {
            return new Vector2(a.x / c, a.y / c);
        }

        public static float SignedDistance(Vector2 a, Vector2 b)
        {
            return (b - a).Length;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            return Math.Abs((b - a).Length);
        }
    }

}
