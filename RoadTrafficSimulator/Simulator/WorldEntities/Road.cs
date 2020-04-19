using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.Interfaces;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    /// <summary>
    /// Represents a road between two intersections, approximated to a straight line
    /// </summary>
    class Road : IRTSUpdateable, IRTSGeometry<Rectangle>
    {
        /// <summary>
        /// Speed limit of the road, in m/s
        /// </summary>
        public readonly float SpeedLimit;

        /// <summary>
        /// Source intersection
        /// </summary>
        public FourWayIntersection SourceIntersection { get; private set; }

        /// <summary>
        /// Target intersection
        /// </summary>
        public FourWayIntersection TargetIntersection { get; private set; }

        /// <summary>
        /// Number of lanes going from target intersection to source intersection
        /// </summary>
        public int NumInLanes { get; }

        /// <summary>
        /// Number of lanes going from source intersection to target intersection
        /// </summary>
        public int NumOutLanes { get; }

        /// <summary>
        /// Total number of lanes
        /// </summary>
        public int NumLanes => NumInLanes + NumOutLanes;

        /// <summary>
        /// Lanes going from target intersection to source intersection
        /// </summary>
        public Lane[] InLanes { get; private set; }

        /// <summary>
        /// Lanes going from source intersection to target intersection
        /// </summary>
        public Lane[] OutLanes { get; private set; }

        /// <summary>
        /// Road direction (possible since road is assumed to be straight line between intersections)
        /// </summary>
        public Vector2 Direction => TargetIntersection.Origin - SourceIntersection.Origin;

        /// <summary>
        /// Tests if direction has a vertical component to determine if road is horizontal
        /// </summary>
        public bool IsHorizontal => Direction.Y == 0;

        /// <summary>
        /// Side of source intersection corresponding to start of the road
        /// </summary>
        public Segment RoadStartSegment => SourceIntersection.GetRoadSegment(TargetIntersection, RoadWidth);

        /// <summary>
        /// Side of target intersection corresponding to start of the road
        /// </summary>
        public Segment RoadTargetSegment => TargetIntersection.GetRoadSegment(SourceIntersection, RoadWidth);

        /// <summary>
        /// Road midline, line between in and out lanes
        /// </summary>
        public Segment RoadMidline {
            get
            {
                Vector2 sepSrc = RoadStartSegment.Lerp(NumOutLanes * Lane.LANE_WIDTH / RoadStartSegment.Length);
                Vector2 sepDst = RoadTargetSegment.Lerp(NumInLanes * Lane.LANE_WIDTH/ RoadTargetSegment.Length);
                return new Segment(sepSrc, sepDst);
            }
        }

        /// <summary>
        /// Source segment of lanes going from source to target intersection
        /// </summary>
        public Segment OutLanesSourceSegment => new Segment(RoadStartSegment.Source, RoadMidline.Source);

        /// <summary>
        /// Target segment of lanes going from source to target intersection
        /// </summary>
        public Segment OutLanesTargetSegment => new Segment(RoadTargetSegment.Target, RoadMidline.Target);

        /// <summary>
        /// Source segment of lanes going from target to source ntersection
        /// </summary>
        public Segment InLanesSourceSegment => new Segment(RoadTargetSegment.Source, RoadMidline.Target);

        /// <summary>
        /// Target segment of lanes going from target to source ntersection
        /// </summary>
        public Segment InLanesTargetSegment => new Segment(RoadStartSegment.Target, RoadMidline.Source);

        /// <summary>
        /// Center of the road, average of the two intersections positions
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return (RoadStartSegment.Midpoint + RoadTargetSegment.Midpoint) / 2;
            }
            set { /* Nothing to do here, you can't set the position of a road, you can try, but it will be pointless. */ }
        }

        /// <summary>
        /// Road width, in meters
        /// </summary>
        public float RoadWidth => NumInLanes * Lane.LANE_WIDTH + NumOutLanes * Lane.LANE_WIDTH;

        /// <summary>
        /// Road length, in meters - based on intersection dimensions
        /// </summary>
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
        /// <param name="source">Source intersection</param>
        /// <param name="target">Target intersection</param>
        /// <param name="numInLanes">Numlanes along negative orientation</param>
        /// <param name="numOutLanes">Numlanes along positive orientation</param>
        /// <param name="speedLimit">Speed limit</param>
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

        /// <summary>
        /// Initialize lanes heading the same direction, supply neighbor information, and target intersection info
        /// </summary>
        /// <param name="numLanes">Number of lanes</param>
        /// <param name="laneTargetIntersection">Target intersection</param>
        /// <returns></returns>
        private Lane[] InitLanes(int numLanes, FourWayIntersection laneTargetIntersection)
        {
            // Create the lanes
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

        /// <summary>
        /// Compute lane geometry based on intersection dimensions
        /// </summary>
        public void ComputeLaneGeometry()
        {
            // In lanes go from top half of target segment, to bottom half of source segment
            SetLaneSourceAndTargets(OutLanes, OutLanesSourceSegment, OutLanesTargetSegment);

            // Out lanes go from top half of source segment, to bottom half of target segment
            SetLaneSourceAndTargets(InLanes, InLanesSourceSegment, InLanesTargetSegment);
        }

        /// <summary>
        /// Set lane source and target segments
        /// </summary>
        /// <param name="lanes">Lane array (in or out lanes)</param>
        /// <param name="sourceSegment">Source segment of lanes</param>
        /// <param name="targetSegment">Target segment of lanes</param>
        private void SetLaneSourceAndTargets(Lane[] lanes, Segment sourceSegment, Segment targetSegment)
        {
            int numLanes = lanes.Length;

            // Split the source and target segments into the approripate amount of subsegments
            Segment[] sourceSubSegment = sourceSegment.SplitSegment(numLanes);
            Segment[] targetSubSegment = targetSegment.SplitSegment(numLanes);

            for (int i = 0; i < lanes.Length; i++)
            {
                lanes[lanes.Length - 1 - i].SourceSegment = sourceSubSegment[i];
                lanes[lanes.Length - 1 - i].TargetSegment = targetSubSegment[i];
            }
        }

        /// <summary>
        /// Update the road, which updates the lanes
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            // Update all the lanes with new vehicle information
            foreach (Lane l in InLanes) l.Update(deltaTime);
            foreach (Lane l in OutLanes) l.Update(deltaTime);
        }

        /// <summary>
        /// Geometrical representation of a road is a rectangle
        /// </summary>
        public Rectangle GetGeometricalFigure() => new Rectangle(Position, RoadWidth, RoadLength, Direction.Angle - (float)Math.PI / 2); // Minus 90° cuz direction is along Y-axis
    }
}
