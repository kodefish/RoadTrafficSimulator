using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DataStructures.Geometry
{
    class Segment
    {
        private readonly Vector2 source, target;

        public Segment(Vector2 source, Vector2 target)
        {
            this.source = source;
            this.target = target;
        }

        public Vector2 Source { get => source; }
        public Vector2 Target { get => target; }
        public Vector2 Vector { get { return target - source; } }
        public float Length { get { return Vector.Length; } }
        public Vector2 Direction { get { return Vector.Normalized; } }
        
        // Returns a point on the segment, dist is how far along the segment in percenteage
        public Vector2 GetPointOnSegment(float dist)
        {
            if (dist > 1) throw new ArgumentOutOfRangeException(
                String.Format("{0} is out of range (0-1)!", dist));
            return source + Direction * dist;
        }

        public Segment SubSegment(float a, float b)
        {
            Vector2 s = GetPointOnSegment(a);
            Vector2 t = GetPointOnSegment(b);
            return new Segment(s, t);
        }

        public Segment[] SplitSegment(int numSubSegments)
        {
            Segment[] subSegments = new Segment[numSubSegments];
            float subSegmentLength = Length / numSubSegments;

            int count = 0;
            for (float i = 0; i < Length; i += subSegmentLength)
            {
                subSegments[count++] = SubSegment(i, i + subSegmentLength);
            }

            return subSegments;
        }
    }
}
