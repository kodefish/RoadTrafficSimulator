using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.Interfaces;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    enum RoadOrientation
    {
        Vertical, Horizontal
    }

    class Road : IRTSUpdateable, IRTSGeometry<Rectangle>
    {
        private readonly float speedLimit;

        // Road Geometry
        // Code contract : source.Origin.X < target.Origin.X for horizontal streets
        // Code contract : source.Origin.Y < target.Origin.Y for vertical streets
        public FourWayIntersection SourceIntersection { get; private set; }
        public FourWayIntersection TargetIntersection { get; private set; }

        // Lane information
        public int NumLanesSouthBound { get; }
        public int NumLanesNorthBound { get; }
        public int NumLanes => NumLanesSouthBound + NumLanesNorthBound;

        public Lane[] SouthBoundLanes { get; private set; }
        public Lane[] NorthBoundLanes { get; private set; }

        // Road orientation
        public RoadOrientation Orientation { get; }
        public bool IsHorizontal => Orientation == RoadOrientation.Horizontal;

        // Road dimensions
        public Vector2 Dimensions 
        {
            get
            {
                if (Orientation == RoadOrientation.Vertical) return new Vector2(RoadWidth, RoadLength);
                else  return new Vector2(RoadLength, RoadWidth);
            }
        }

        // Road position
        public Segment RoadStartSegment => SourceIntersection.GetRoadSegment(TargetIntersection);
        public Segment RoadTargetSegment => TargetIntersection.GetRoadSegment(SourceIntersection);
        public Vector2 Position
        {
            get
            {
                return (RoadStartSegment.Midpoint + RoadTargetSegment.Midpoint) / 2;
            }
            set { /* Nothing to do here, you can't set the position of a road, you can try, but it will be pointless. */ }
        }

        // Road dimensions
        public float RoadWidth => NumLanesSouthBound * Lane.LANE_WIDTH + NumLanesNorthBound * Lane.LANE_WIDTH;
        public float RoadLength
        {
            get
            {
                Vector2 srcIntersectionOffset, targetIntersectionOffset;
                if (Orientation == RoadOrientation.Vertical)
                {
                    srcIntersectionOffset = new Vector2(0, SourceIntersection.Height / 2);
                    targetIntersectionOffset = new Vector2(0, TargetIntersection.Height / 2);
                }
                else if (Orientation == RoadOrientation.Horizontal)
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
        /// <param name="numLanesSouthBound">Numlanes along negative orientation</param>
        /// <param name="numLanesNorthBound">Numlanes along positive orientation</param>
        /// <param name="orientation"></param>
        public Road(
            ref FourWayIntersection source, ref FourWayIntersection target, 
            int numLanesSouthBound, int numLanesNorthBound, 
            RoadOrientation orientation, float speedLimit)
        {
            Orientation = orientation;
            this.speedLimit = speedLimit;

            // Make sure road points along positive X or Y axis, and are aligned along the orientation
            Vector2 sourceOrigin = source.Origin;
            Vector2 targetOrigin = target.Origin;
            if (orientation == RoadOrientation.Vertical && sourceOrigin.X == targetOrigin.X)
            {
                SourceIntersection = sourceOrigin.Y < targetOrigin.Y ? source : target;
                TargetIntersection = sourceOrigin.Y < targetOrigin.Y ? target : source;
            } else if (orientation == RoadOrientation.Horizontal && sourceOrigin.Y == targetOrigin.Y)
            {
                SourceIntersection = sourceOrigin.X < targetOrigin.X ? source : target;
                TargetIntersection = sourceOrigin.X < targetOrigin.X ? target : source;
            }
            else
            {
                throw new ArgumentException("Alignement of intersections does not correspond to orientation!");
            }

            this.NumLanesSouthBound = numLanesSouthBound;
            this.NumLanesNorthBound = numLanesNorthBound;
            SouthBoundLanes = InitLanes(numLanesSouthBound);
            NorthBoundLanes = InitLanes(numLanesNorthBound);

            SourceIntersection.AddRoad(this);
            TargetIntersection.AddRoad(this);
        }

        private Lane[] InitLanes(int numLanes)
        {
            Lane[] lanes = new Lane[numLanes];
            for (int i = 0; i < numLanes; i++) lanes[i] = new Lane(speedLimit);
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
            // Reverse the src segments since we start with south bound lanes on the left
            Segment[] srcSubSegment = RoadStartSegment.SplitSegment(NumLanes, true);
            Segment[] targetSubSegment = RoadTargetSegment.SplitSegment(NumLanes, false);

            // Set the source segments of the sourth bound lanes: targetIntersection -> sourceIntersection (left)
            int i = 0;
            for (; i < NumLanesSouthBound; i++)
            {
                SouthBoundLanes[i].SourceSegment = targetSubSegment[i];
                SouthBoundLanes[i].TargetSegment = srcSubSegment[i];
            }

            // Set the source segments of the north bound lanes: sourceIntersection -> targetIntersection (right)
            for (; i < NumLanes; i++)
            {
                NorthBoundLanes[i- NumLanesSouthBound].SourceSegment = srcSubSegment[i];
                NorthBoundLanes[i- NumLanesSouthBound].TargetSegment = targetSubSegment[i];
            }
        }

        public void Update(float deltaTime)
        {
            // Update all the lanes with new car information
            foreach (Lane l in SouthBoundLanes) l.Update(deltaTime);
            foreach (Lane l in NorthBoundLanes) l.Update(deltaTime);
        }

        public Rectangle GetGeometricalFigure()
        {
            return new Rectangle(Position, Dimensions.X, Dimensions.Y);
        }
    }
}
