using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.Interfaces;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    class Road : IRTSUpdateable, IRTSGeometry<Rectangle>
    {
        public readonly float SpeedLimit;

        // Road Geometry
        public FourWayIntersection SourceIntersection { get; private set; }
        public FourWayIntersection TargetIntersection { get; private set; }

        // Lane information
        public int NumInLanes { get; }
        public int NumOutLanes { get; }
        public int NumLanes => NumInLanes + NumOutLanes;

        public Lane[] InLanes { get; private set; }
        public Lane[] OutLanes { get; private set; }

        // Road direction
        public Vector2 Direction => TargetIntersection.Origin - SourceIntersection.Origin;
        public bool IsHorizontal => Direction.Y == 0;

        // Road position
        public Segment RoadStartSegment => SourceIntersection.GetRoadSegment(TargetIntersection, RoadWidth);
        public Segment RoadTargetSegment => TargetIntersection.GetRoadSegment(SourceIntersection, RoadWidth);
        public Segment RoadMidline {
            get
            {
                Vector2 sepSrc = RoadStartSegment.Lerp(NumOutLanes * Lane.LANE_WIDTH / RoadStartSegment.Length);
                Vector2 sepDst = RoadTargetSegment.Lerp(NumInLanes * Lane.LANE_WIDTH/ RoadTargetSegment.Length);
                return new Segment(sepSrc, sepDst);
            }
        }

        public Segment OutLanesSourceSegment => new Segment(RoadStartSegment.Source, RoadMidline.Source);
        public Segment OutLanesTargetSegment => new Segment(RoadTargetSegment.Target, RoadMidline.Target);

        public Segment InLanesSourceSegment => new Segment(RoadTargetSegment.Source, RoadMidline.Target);
        public Segment InLanesTargetSegment => new Segment(RoadStartSegment.Target, RoadMidline.Source);
        public Vector2 Position
        {
            get
            {
                return (RoadStartSegment.Midpoint + RoadTargetSegment.Midpoint) / 2;
            }
            set { /* Nothing to do here, you can't set the position of a road, you can try, but it will be pointless. */ }
        }

        // Road dimensions
        public float RoadWidth => NumInLanes * Lane.LANE_WIDTH + NumOutLanes * Lane.LANE_WIDTH;
        public float RoadLength
        {
            get
            {
                Vector2 srcIntersectionOffset, targetIntersectionOffset;
                if (!IsHorizontal)
                {
                    srcIntersectionOffset = new Vector2(0, SourceIntersection.Height / 2);
                    targetIntersectionOffset = new Vector2(0, TargetIntersection.Height / 2);
                }
                else if (IsHorizontal)
                {
                    srcIntersectionOffset = new Vector2(SourceIntersection.Width / 2, 0);
                    targetIntersectionOffset = new Vector2(TargetIntersection.Width / 2, 0);
                } else
                {
                    throw new InvalidOperationException("No road orientation specified!");
                }

                Vector2 roadStart = SourceIntersection.Origin + srcIntersectionOffset;
                Vector2 roadEnd = TargetIntersection.Origin- targetIntersectionOffset;
                return Vector2.Distance(roadStart, roadEnd);
            }
        }


        /// <summary>
        /// Creates a the road between 2 intersections
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="numInLanes">Numlanes along negative orientation</param>
        /// <param name="numOutLanes">Numlanes along positive orientation</param>
        /// <param name="orientation"></param>
        public Road(
            ref FourWayIntersection source, ref FourWayIntersection target, 
            int numInLanes, int numOutLanes, 
            float speedLimit)
        {
            this.SpeedLimit = speedLimit;

            SourceIntersection = source;
            TargetIntersection = target;

            this.NumInLanes = numInLanes;
            this.NumOutLanes = numOutLanes;
            InLanes = InitLanes(numInLanes, SourceIntersection);
            OutLanes = InitLanes(numOutLanes, TargetIntersection);

            SourceIntersection.AddRoad(this);
            TargetIntersection.AddRoad(this);
        }

        private Lane[] InitLanes(int numLanes, FourWayIntersection laneTargetIntersection)
        {
            Lane[] lanes = new Lane[numLanes];
            for (int i = 0; i < numLanes; i++) lanes[i] = new Lane(i, SpeedLimit, laneTargetIntersection);

            // Setup lane neighbors
            for (int i = 0; i < numLanes; i++)
            {
                // Get lane neighbors
                Lane[] neighboringLanes;
                if (0 < i && i < numLanes - 1)
                {
                    neighboringLanes = new Lane[] {
                        lanes[i - 1],
                        lanes[i + 1]
                    };
                }
                else if (numLanes > 1)
                {
                    neighboringLanes = new Lane[] {
                        i == 0 ? lanes[i + 1] : lanes[i - 1]
                    };
                }
                else
                {
                    // no neighboring lanes -> empty array
                    neighboringLanes = new Lane[0];
                }

                lanes[i].NeighboringLanes = neighboringLanes;
            }

            return lanes;
        }

        /// <summary>
        /// We're clean bois, always clean up after yourself!
        /// </summary>
        ~Road()
        {
            SourceIntersection.RemoveRoad(this);
            TargetIntersection.RemoveRoad(this);
        }

        public void ComputeLaneGeometry()
        {
            // In lanes go from top half of target segment, to bottom half of source segment
            SetLaneSourceAndTargets(OutLanes, OutLanesSourceSegment, OutLanesTargetSegment);

            // Out lanes go from top half of source segment, to bottom half of target segment
            SetLaneSourceAndTargets(InLanes, InLanesSourceSegment, InLanesTargetSegment);
        }

        private void SetLaneSourceAndTargets(Lane[] lanes, Segment sourceSegment, Segment targetSegment)
        {
            int numLanes = lanes.Length;
            // Reverse target segments
            Segment[] sourceSubSegment = sourceSegment.SplitSegment(numLanes);
            Segment[] targetSubSegment = targetSegment.SplitSegment(numLanes);

            for (int i = 0; i < lanes.Length; i++)
            {
                lanes[lanes.Length - 1 - i].SourceSegment = sourceSubSegment[i];
                lanes[lanes.Length - 1 - i].TargetSegment = targetSubSegment[i];
            }
        }

        public void Update(float deltaTime)
        {
            // Update all the lanes with new car information
            foreach (Lane l in InLanes) l.Update(deltaTime);
            foreach (Lane l in OutLanes) l.Update(deltaTime);
        }

        public Rectangle GetGeometricalFigure() => new Rectangle(Position, RoadWidth, RoadLength, Direction.Angle - (float)Math.PI / 2); // Minus 90° cuz direction is along Y-axis
    }
}
