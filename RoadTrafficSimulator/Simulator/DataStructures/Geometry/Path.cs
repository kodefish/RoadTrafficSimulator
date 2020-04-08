using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    ^/// <summary>
    /// A path is a sequence of consecutive segments, and has a given radius
    /// A point with a distance within the radius is considered to be on the path
    /// </summary>
    class Path
    {
        public float Radius { get; private set; }
        private readonly List<Segment> segments;

        /// <summary>
        /// Constructs path from segments
        /// </summary>
        /// <param name="segments">Segments that make up the center of the path</param>
        /// <param name="radius">Width of the path</param>
        public Path(List<Segment> segments, float radius = 0.1f)
        {
            Radius = radius;
            this.segments = segments ?? throw new ArgumentException("Segments must be non-null;");
        }

        private Segment ClosestSegment(Vector2 position)
        {
            float minDist = float.PositiveInfinity;
            Segment closestSegment = null;
            foreach(Segment s in segments)
            {
                Vector2 normalPt = s.NormalPoint(position);
                float distToNormalPt = Vector2.Distance(normalPt, position);
                if (distToNormalPt < minDist)
                {
                    minDist = distToNormalPt;
                    closestSegment = s;
                }
            }
            return closestSegment;
        }

        public Vector2 NormalPoint(Vector2 p) => ClosestSegment(p).NormalPoint(p);
        public Vector2 Tangent(Vector2 p) => ClosestSegment(p).Direction;

        class Builder
        {
            private readonly float radius;
            List<Segment> segments;

            public Builder(float radius = 0.1f) {
                this.radius = radius;
                segments = new List<Segment>();
            }

            public void AddSegment(Segment s) => segments.Add(s);

            /// <summary>
            /// Samples a bezier curve to for subsegments
            /// </summary>
            /// <param name="c">Curve to sample</param>
            /// <param name="numSamples">Number of sample, default is 10</param>
            public void AddBezierCurve(BezierCurve c, float numSamples = 10)
            {
                if (numSamples < 1) throw new ArgumentException(String.Format("Number of samples ({0}) must not be < 1!", numSamples));
                float sampleRate = 1 / numSamples;
                for (float i = 0; i < 1 - sampleRate; i += sampleRate)
                    segments.Add(new Segment(c.GetPosition(i), c.GetPosition(i + sampleRate)));
            }

            // TODO prevent building non-contiguous paths
            public Path Build() => new Path(segments, radius);
        }
    }

}
