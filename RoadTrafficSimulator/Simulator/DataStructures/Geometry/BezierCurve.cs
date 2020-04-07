using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    /// <summary>
    /// Bezier curve representation. The curve goes from one anchor point to the next,
    /// the control points are used to specify the tangent at each corresponding
    /// anchor point. This is also know as a cubic interpolation
    /// </summary>
    class BezierCurve : GeometricalFigure
    {
        // Anchor and control points.
        private readonly Vector2 a1, c1, a2, c2;

        /// <summary>
        /// Constructs a cubic interpolation between two points. (anchor, control) represents the tangent at the anchor point
        /// </summary>
        /// <param name="a1">Anchor point 1</param>
        /// <param name="c1">Control point 1</param>
        /// <param name="a2">Anchor point 2</param>
        /// <param name="c2">Control point 2</param>
        public BezierCurve(Vector2 a1,Vector2 c1, Vector2 a2,Vector2 c2)
        {
            this.a1 = a1;
            this.c1 = c1;
            this.a2 = a2;
            this.c2 = c2;
        }

        private float length = -1;
        public float Length
        {
            get
            {
                // only compute length once, as integrating along the curve can get expensive
                if (length < 0)
                {
                    length = DistanceAlongCurve(0, 1);
                }
                return length;
            }
        }

        public float DistanceAlongCurve(float a, float b)
        {
            if (!(0 < a && a < b && b < 1)) throw new ArgumentOutOfRangeException(String.Format("{0} and {1} must by between 0 and 1, {0} < {1}!", a, b));
            float length = 0;
            float step = (b - a) / 100;
            Vector2 prevPos = GetPosition(a);
            for (float t = a + step; t <= b; t += step)
            {
                Vector2 newPos = GetPosition(t);
                length += Vector2.Distance(prevPos, newPos);
            }
            return length;
        }

        /// <summary>
        /// Returns position at time step t along the Bezier curve defined by the four support points. At 
        /// t = 0, the curve returns the starting anchor point, at t=1, the target anchor point. Between 
        /// it's a nice and smooth curve, the smoothness of which is controlled by the control points.
        /// </summary>
        /// <param name="t">step, between 0 and 1</param>
        public Vector2 GetPosition(float t)
        {
            if (t > 1) throw new ArgumentOutOfRangeException(String.Format("{0} is out of range (0-1).", t));

            float t2 = t * t;
            float t3 = t2 * t;
            Vector2 interpolatedPosition =
                (a2 + (c1 - c2) * 3 - a1) * t3 +
                (a1 - c1 * 2 + c2) * 3 * t2 +
                (c1 - a1) * 3 * t +
                a1;
            return interpolatedPosition;
        }

        /// <summary>
        /// Returns tangent of Bezier curve, f'(t)
        /// </summary>
        public Vector2 GetTangent(float t)
        {
            if (t > 1) throw new ArgumentOutOfRangeException(String.Format("{0} is out of range (0-1).", t));

            float t2 = t * t;
            float t3 = t2 * t;
            Vector2 interpolatedPosition =
                (a2 + (c1 - c2) * 3 - a1) * 3 *t2 +
                (a1 - c1 * 2 + c2) * 6 * t +
                (c1 - a1) * 3;
            return interpolatedPosition;
        }
    }
}
