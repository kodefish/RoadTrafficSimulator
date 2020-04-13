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
        public float Length { get; }

        /// <summary>
        /// Constructs path from segments
        /// </summary>
        /// <param name="segments">Segments that make up the center of the path</param>
        /// <param name="radius">Width of the path</param>
        private Path(List<Segment> segments, float radius = 0.1f)
        {
            if (radius < 0) throw new ArgumentException("Radius must be positive");
            if (segments == null) throw new ArgumentException("Segments must be non-null");
            if (segments.Count == 0) throw new ArgumentException("Segments must be non-empty");

            Radius = radius;
            Segments = segments.ToArray();
            Length = 0;
            foreach (Segment s in segments) Length += s.Length;
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
                segments.Add(new Segment(c.GetPosition(Math.Min(i, 1)), c.GetPosition(Math.Min(i + sampleRate, 1))));
            return new Path(segments, radius);
        }

        public static Path TransitionPath(Path source, Path target, Vector2 origin, float forwardOffset)
        {
            Segment closestSourceSegment = source.Segments[source.ClosestSegment(origin)];
            Vector2 startingPos = closestSourceSegment.ProjectOntoSupportingLine(origin);

            Segment closestTargetSegment = target.Segments[source.ClosestSegment(origin)];
            Vector2 normalPoint = closestTargetSegment.ProjectOntoSupportingLine(origin); 
            Vector2 targetPos = normalPoint + closestTargetSegment.Direction * forwardOffset;

            BezierCurve bezierCurve = new BezierCurve(
                startingPos, startingPos + closestSourceSegment.Direction,
                targetPos, targetPos + closestTargetSegment.Direction);

            return FromBezierCurve(bezierCurve);
        }

        /// <summary>
        /// Start of the path, source point in the first segment
        /// </summary>
        public Vector2 PathStart => Segments[0].Source;

        /// <summary>
        /// End of the path, target point in last segment
        /// </summary>
        public Vector2 PathEnd => Segments[Segments.Length - 1].Target;

        /// <summary>
        /// Computes the closest segment to a position. Distance is measured along perpendicular line from the point to the segment
        /// </summary>
        /// <param name="position">Position in the world</param>
        /// <returns>Closest segment to the position along the path</returns>
        private int ClosestSegment(Vector2 position)
        {
            float minDist = float.PositiveInfinity;
            int idx = -1;
            for (int i = 0; i < Segments.Length; i++)
            {
                Vector2 normalPt = Segments[i].ProjectOntoSupportingLine(position);
                float distToNormalPt = Vector2.Distance(normalPt, position);
                if (distToNormalPt < minDist)
                {
                    minDist = distToNormalPt;
                    idx = i;
                }
            }
            return idx;
        }

        /// <summary>
        /// Projects a position onto the closest segment along the path
        /// </summary>
        /// <param name="p">Point to project</param>
        /// <returns>Projected point</returns>
        public Vector2 NormalPoint(Vector2 p) => Segments[ClosestSegment(p)].ProjectOntoSupportingLine(p);

        /// <summary>
        /// Distance of a point p to the path (distance from p to p's projection)
        /// </summary>
        /// <param name="p">Point to project</param>
        /// <returns>Orthogonal distance to closest segment on the path</returns>
        public float DistanceToPath(Vector2 p) => Vector2.Distance(p, NormalPoint(p));

        /// <summary>
        /// Computes the tangent vector of a projected point along the path. Since the path 
        /// is a series of segments, the tangent is the direction of the closest segment.
        /// This is an approximation, but should be accurate enough if the sampling rate is high enough
        /// </summary>
        /// <param name="p">Point to project</param>
        /// <returns>Tangent along the path</returns>
        public Vector2 TangentOfProjectedPosition(Vector2 p) => Segments[ClosestSegment(p)].Direction;

        /// <summary>
        /// Distance of a projected point from the start of the path.
        /// </summary>
        /// <param name="position">Point to project</param>
        /// <returns>Distance of projection along the path</returns>
        public float DistanceOfProjectionAlongPath(Vector2 position)
        {
            // Get segment closest to position
            int segIdx = ClosestSegment(position);

            // Sum up all the segments before it
            float distance = 0;
            for (int i = 0; i < segIdx; i++) distance += Segments[i].Length;

            // Add scalar projection of point onto
            distance += Vector2.Distance(Segments[segIdx].Source, Segments[segIdx].ProjectOntoSupportingLine(position));
            return distance;
        }

        public Vector2 Lerp(float lerpOffset)
        {
            if (lerpOffset > 1) throw new ArgumentOutOfRangeException(String.Format("Lerp offset ({0}) must be between 0 and 1!", lerpOffset));
            float distAlongPath = Length * lerpOffset;

            // Skip over all the segments that are covered already
            int i = 0;
            while(Segments[i].Length < distAlongPath)
            {
                distAlongPath -= Segments[i].Length;;
                i++;
            }

            return Segments[i].Lerp(distAlongPath / Segments[i].Length);
        }

        /// <summary>
        /// Inverse lerp operation by approximating the position along the path by projecting 
        /// onto closest segment, and computing the distance along the path.
        /// </summary>
        public float InverseLerp(Vector2 position) => DistanceOfProjectionAlongPath(position) / Length;
    }
}
