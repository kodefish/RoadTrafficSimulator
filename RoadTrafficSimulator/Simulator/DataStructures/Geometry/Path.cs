using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    /// <summary>
    /// A path is a sequence of consecutive segments, sampled from a Bezier curve, and has a given radius
    /// A point with a distance within the radius is considered to be on the path
    /// </summary>
    class Path
    {
        public float Radius { get; private set; }
        public Segment[] Segments { get; private set; }

        /// <summary>
        /// Constructs path from segments
        /// </summary>
        /// <param name="segments">Segments that make up the center of the path</param>
        /// <param name="radius">Width of the path</param>
        private Path(List<Segment> segments, float radius = 0.1f)
        {
            if (radius < 0) throw new ArgumentException("Radius must be positive");
            else Radius = radius;

            if (segments == null) throw new ArgumentException("Segments must be non-null");
            else {
                if (segments.Count == 0) throw new ArgumentException("Segments must be non-empty");
                else Segments = segments.ToArray();
            }
        }
        /// <summary>
        /// Samples a bezier curve to for subsegments and creates a path
        /// </summary>
        /// <param name="c">Curve to sample</param>
        /// <param name="numSamples">Number of sample, default is 10</param>
        /// <param name="Radius">Radius of the path</param>
        public static Path FromBezierCurve(BezierCurve c, float numSamples = 10, float radius = 0.1f)
        {
            if (numSamples < 1) throw new ArgumentException(String.Format("Number of samples ({0}) must not be < 1!", numSamples));
            float sampleRate = 1 / numSamples;
            List<Segment> segments = new List<Segment>();
            for (float i = 0; i < 1; i += sampleRate)
                segments.Add(new Segment(c.GetPosition(i), c.GetPosition(i + sampleRate)));
            return new Path(segments, radius);
        }

        public Vector2 PathStart => Segments[0].Source;
        public Vector2 PathEnd => Segments[Segments.Length - 1].Target;

        private int ClosestSegment(Vector2 position)
        {
            float minDist = float.PositiveInfinity;
            int idx = -1;
            for (int i = 0; i < Segments.Length; i++)
            {
                Vector2 normalPt = Segments[i].NormalPoint(position);
                float distToNormalPt = Vector2.Distance(normalPt, position);
                if (distToNormalPt < minDist)
                {
                    minDist = distToNormalPt;
                    idx = i;
                }
            }
            return idx;
        }

        public Vector2 NormalPoint(Vector2 p) => Segments[ClosestSegment(p)].NormalPoint(p);
        public Vector2 TangentOfProjectedPosition(Vector2 p) => Segments[ClosestSegment(p)].Direction;

        public float DistanceOfProjectionAlongPath(Vector2 position)
        {
            // Get segment closest to position
            int segIdx = ClosestSegment(position);

            // Sum up all the segments before it
            float distance = 0;
            for (int i = 0; i < segIdx; i++) distance += Segments[i].Length;

            // Add scalar projection of point onto
            distance += Segments[segIdx].NormalPoint(position).Length / Segments[segIdx].Length;
            return distance;
        }
    }
}
