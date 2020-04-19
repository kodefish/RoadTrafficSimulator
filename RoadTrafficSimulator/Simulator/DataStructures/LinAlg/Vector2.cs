using System;
namespace RoadTrafficSimulator.Simulator.DataStructures.LinAlg
{
    class Vector2
    {
        /// <summary>
        /// Vector (1, 0)
        /// </summary>
        public static Vector2 UnitX => new Vector2(1, 0);

        /// <summary>
        /// Vector (0, 1)
        /// </summary>
        public static Vector2 UnitY => new Vector2(0, 1);

        /// <summary>
        /// UnitY
        /// </summary>
        public static Vector2 Up => UnitY;

        /// <summary>
        /// -UnitY
        /// </summary>
        public static Vector2 Down => -UnitY;

        /// <summary>
        /// UnitX
        /// </summary>
        public static Vector2 Right => UnitX;

        /// <summary>
        /// -UnitX
        /// </summary>
        public static Vector2 Left => -UnitX;

        /// <summary>
        /// X component of the vector
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Y component of the vector
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Creates a vector (0, 0)
        /// </summary>
        public Vector2() => new Vector2(0, 0);

        /// <summary>
        /// Creates a 2-D vector
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Euclidian norm of the vector
        /// </summary>
        public float Norm => (float)Math.Sqrt(X * X + Y * Y);

        /// <summary>
        /// Normalized version of the vector
        /// </summary>
        public Vector2 Normalized => this / Norm;

        /// <summary>
        /// Normal as defined by (-b, a)
        /// </summary>
        public Vector2 Normal => new Vector2(-Y, X).Normalized;

        /// <summary>
        /// Angle w.r.t the horizontal axis, mapped from -Pi to Pi.
        /// </summary>
        public float Angle
        {
            get {
                float cosAngle = Vector2.Dot(this, Vector2.UnitX) / this.Norm;
                float angle = (float)Math.Acos(cosAngle);
                return Y < 0 ? (float)(2 * Math.PI - angle) : angle; // Check the orientation of Y to determine if the angle is positive or not, needed because cos(x) = cos(-x)
            }
        }

        // Operator on single vector
        public static Vector2 operator +(Vector2 a) => a;
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);

        // Vector addition, subtraction, scalar multiplication and scalar division
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float c) => new Vector2(a.X * c, a.Y * c);
        public static Vector2 operator *(float c, Vector2 a) => new Vector2(a.X * c, a.Y * c);
        public static Vector2 operator /(Vector2 a, float c) => new Vector2(a.X / c, a.Y / c);

        /// <summary>
        /// Absolute distance between two vectors
        /// </summary>
        public static float Distance(Vector2 a, Vector2 b) => Math.Abs((b - a).Norm);

        /// <summary>
        /// Dot product of two vectors (a . b)
        /// </summary>
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// Apply rotation alpha to some vector
        /// </summary>
        /// <param name="a">Vector to rotate</param>
        /// <param name="alpha">Angle to rotate by</param>
        /// <returns></returns>
        public static Vector2 Rotate(Vector2 a, float alpha)
            => new Vector2((float) (a.X * Math.Cos(alpha) - a.Y * Math.Sin(alpha)),
                (float) (a.X * Math.Sin(alpha) + a.Y * Math.Cos(alpha)));

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        /// <summary>
        /// Compares two vectors. Equal if components are the same
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>true if obj is a Vector2 of same components</returns>
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

        /// <summary>
        /// Hash code computation
        /// </summary>
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
