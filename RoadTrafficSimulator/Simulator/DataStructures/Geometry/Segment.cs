using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    class Segment : GeometricalFigure
    {
        /// <summary>
        /// Source point of the segment
        /// </summary>
        public Vector2 Source { get; }

        /// <summary>
        /// Target point of the segment
        /// </summary>
        public Vector2 Target { get; }

        /// <summary>
        /// Creates a segment from source to target
        /// </summary>
        /// <param name="source">Source of the segment</param>
        /// <param name="target">Target of the segment</param>
        public Segment(Vector2 source, Vector2 target)
        {
            Source = source;
            Target = target;
        }

        /// <summary>
        /// Vector form of the segment
        /// </summary>
        public Vector2 Vector { get { return Target - Source; } }

        /// <summary>
        /// Euclidian norm of the segment
        /// </summary>
        public float Length { get { return Vector.Length; } }

        /// <summary>
        /// Direction, from source to target. Normalized
        /// </summary>
        public Vector2 Direction { get { return Vector.Normalized; } }

        /// <summary>
        /// Point in the middle of the segment
        /// </summary>
        public Vector2 Midpoint => (Target + Source) / 2;
        
        // Returns a point on the segment, dist is how far along the segment in percenteage
        /// <summary>
        /// Linear interpolation along the segment
        /// </summary>
        /// <param name="dist">Lerp param</param>
        /// <returns>Lerped point</returns>
        public Vector2 Lerp(float dist)
        {
            if (dist > 1) throw new ArgumentOutOfRangeException(
                String.Format("{0} is out of range (0-1)!", dist));
            return Source + Vector * dist;
        }

        /// <summary>
        /// Creates a subsegmnt between two linearly interpolated positions
        /// </summary>
        /// <param name="a">Lerp factor for source of subsegment</param>
        /// <param name="b">Lerp factor for target of subsegment</param>
        /// <returns>Subsegment from Lerp(a) to Lerp(b)</returns>
        public Segment SubSegment(float a, float b)
        {
            Vector2 s = Lerp(a);
            Vector2 t = Lerp(b);
            return new Segment(s, t);
        }

        /// <summary>
        /// Splits the segment into even subsegments. Reversed flag flips 
        /// source and target of the segment (also flips the normals of the subsegments)
        /// </summary>
        /// <param name="numSubSegments">Number of subsegments to create</param>
        /// <param name="reversed">Flips source and target</param>
        /// <returns></returns>
        public Segment[] SplitSegment(int numSubSegments, bool reversed)
        {
            if (reversed) new Segment(Target, Source).SplitSegment(numSubSegments, !reversed);
            Segment[] subSegments = new Segment[numSubSegments];

            for (float i = 0; i < subSegments.Length; i++)
            {
                subSegments[(int)i] = SubSegment(i / numSubSegments, (i + 1) / numSubSegments);
            }
            return subSegments;
        }

        /// <summary>
        /// Test if point on segment
        /// True if point is collinear to source and target, and is between the two
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>True if point on segment</returns>
        public bool PointOnSegment(Vector2 point)
        {
            return Vector2.Distance(Source, point) + Vector2.Distance(point, Target) == Vector2.Distance(Source, Target);
        }

        /// <summary>
        /// Projects point onto supporting line of the segment
        /// </summary>
        /// <param name="p">Point to project</param>
        /// <returns></returns>
        public Vector2 ProjectOntoSupportingLine(Vector2 p)
        {
            Vector2 aP = p - Source;
            Vector2 normalPt = Source + Direction * Vector2.Dot(aP, Direction);
            return normalPt;
        }

        /// <summary>
        /// Inverse lerp.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Lerp value of point</returns>
        public float InverseLerp(Vector2 point)
        {
            if (!PointOnSegment(point)) throw new ArgumentException(String.Format("{0} is not on the segment!", point));
            return Vector2.Distance(Source, point) / Vector2.Distance(Source, Target);
        }

        public override string ToString()
        {
            return String.Format("Segment from: {0} to {1}", Source, Target);
        }
    }
}
