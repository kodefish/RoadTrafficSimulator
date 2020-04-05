using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    class BezierCurve
    {
        private readonly Vector2 supportPoint1, supportPoint2, supportPoint3, supportPoint4;

        public BezierCurve(Vector2 supportPoint1,Vector2 supportPoint2,Vector2 supportPoint3,Vector2 supportPoint4)
        {
            this.supportPoint1 = supportPoint1;
            this.supportPoint2 = supportPoint2;
            this.supportPoint3 = supportPoint3;
            this.supportPoint4 = supportPoint4;
        }

        private float length = -1;
        public float Length
        {
            get
            {
                // only compute length once, as integrating along the curve can get expensive
                if (length < 0)
                {
                    length = 0;
                    float step = 0.1f;
                    Vector2 prevPos = GetPosition(0);
                    for (float t = step; t <= 1; t += step)
                    {
                        Vector2 newPos = GetPosition(t);
                        length += Vector2.Distance(prevPos, newPos);
                    }

                }
                return length;
            }
        }

        // Returns position at time step t along the Bezier curve defined by the four support points
        // Explicit formula for the curve is used, but it's basically a bunch of Lerps wrote out
        public Vector2 GetPosition(float t)
        {
            if (t > 1) throw new ArgumentOutOfRangeException(String.Format("{0} is out of range (0-1).", t));
            Vector2 interpolatedPosition =
                supportPoint1 * (1 - t) * (1 - t) * (1 - t) +
                supportPoint2 * 3 * (1 - t) * (1 - t) * t +
                supportPoint3 * 3 * (1 - t) * t * t +
                supportPoint4 * t * t * t;
            return interpolatedPosition;
        }
    }
}
