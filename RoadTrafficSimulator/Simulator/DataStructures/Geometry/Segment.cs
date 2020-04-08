using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    class Segment : GeometricalFigure
    {
        public Vector2 Source { get; }
        public Vector2 Target { get; }

        public Segment(Vector2 source, Vector2 target)
        {
            Source = source;
            Target = target;
        }

        public Vector2 Vector { get { return Target - Source; } }
        public float Length { get { return Vector.Length; } }
        public Vector2 Direction { get { return Vector.Normalized; } }
        public Vector2 Midpoint => (Target + Source) / 2;
        
        // Returns a point on the segment, dist is how far along the segment in percenteage
        public Vector2 GetPointOnSegment(float dist)
        {
            if (dist > 1) throw new ArgumentOutOfRangeException(
                String.Format("{0} is out of range (0-1)!", dist));
            return Source + Vector * dist;
        }

        public Segment SubSegment(float a, float b)
        {
            Vector2 s = GetPointOnSegment(a);
            Vector2 t = GetPointOnSegment(b);
            return new Segment(s, t);
        }

        public Segment[] SplitSegment(int numSubSegments, bool reversed)
        {
            Segment[] subSegments = new Segment[numSubSegments];

            for (float i = 0; i < subSegments.Length; i++)
            {
                subSegments[(int)i] = SubSegment(i / numSubSegments, (i + 1) / numSubSegments);
            }

            // Reverse array if needed
            if (reversed)
            {
                Segment[] reversedSubSegments = new Segment[numSubSegments];
                for (int i = 0; i < numSubSegments; i++)
                {
                    reversedSubSegments[numSubSegments - 1 - i] = subSegments[i];
                }
                return reversedSubSegments;
            }
            else
                return subSegments;
        }

        public bool PointOnSegment(Vector2 point)
        {
            return Vector2.Distance(Source, point) + Vector2.Distance(point, Target) == Vector2.Distance(Source, Target);
        }

        public Vector2 NormalPoint(Vector2 p)
        {
            Vector2 aP = p - Source;
            Vector2 normalPt = Source + Direction * Vector2.Dot(aP, Direction);
            return normalPt;
        }

        public float ProgressAlongSegment(Vector2 point)
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
