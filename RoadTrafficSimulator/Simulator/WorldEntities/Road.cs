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

    class Road : IRTSDimension, IRTSPosition
    {
        // Code contract : source.Origin.X < target.Origin.X for horizontal streets
        // Code contract : source.Origin.Y < target.Origin.Y for vertical streets
        private FourWayIntersection sourceIntersection, targetIntersection;
        private readonly int numLanesSouthBound;
        private readonly int numLanesNorthBound;
        private int NumLanes => numLanesSouthBound + numLanesNorthBound;

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
        public Vector2 Position
        {
            get { return (sourceIntersection.Origin + targetIntersection.Origin) / 2; }
            set { /* Nothing to do here, you can't set the position of a road, you can try, but it will be pointless. */ }
        }

        // Road dimensions
        public float RoadWidth => numLanesSouthBound * Lane.LANE_WIDTH + numLanesNorthBound * Lane.LANE_WIDTH;
        public float RoadLength
        {
            get
            {
                Vector2 srcIntersectionOffset, targetIntersectionOffset;
                if (Orientation == RoadOrientation.Vertical)
                {
                    srcIntersectionOffset = new Vector2(0, sourceIntersection.Height / 2);
                    targetIntersectionOffset = new Vector2(0, targetIntersection.Height / 2);
                }
                else if (Orientation == RoadOrientation.Horizontal)
                {
                    srcIntersectionOffset = new Vector2(sourceIntersection.Width / 2, 0);
                    targetIntersectionOffset = new Vector2(targetIntersection.Width / 2, 0);
                } else
                {
                    throw new InvalidOperationException("No road orientation specified!");
                }

                Vector2 roadStart = sourceIntersection.Origin + srcIntersectionOffset;
                Vector2 roadEnd = targetIntersection.Origin- targetIntersectionOffset;
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
            RoadOrientation orientation)
        {
            Orientation = orientation;

            // Make sure road points along positive X or Y axis, and are aligned along the orientation
            Vector2 sourceOrigin = source.Origin;
            Vector2 targetOrigin = target.Origin;
            if (orientation == RoadOrientation.Vertical && sourceOrigin.X == targetOrigin.X)
            {
                sourceIntersection = sourceOrigin.Y < targetOrigin.Y ? source : target;
                targetIntersection = sourceOrigin.Y < targetOrigin.Y ? target : source;
            } else if (orientation == RoadOrientation.Horizontal && sourceOrigin.Y == targetOrigin.Y)
            {
                sourceIntersection = sourceOrigin.X < targetOrigin.X ? source : target;
                targetIntersection = sourceOrigin.X < targetOrigin.X ? target : source;
            }
            else
            {
                throw new ArgumentException("Alignement of intersections does not correspond to orientation!");
            }

            this.numLanesSouthBound = numLanesSouthBound;
            this.numLanesNorthBound = numLanesNorthBound;
            SouthBoundLanes = InitLanes(numLanesSouthBound);
            NorthBoundLanes = InitLanes(numLanesNorthBound);

            sourceIntersection.AddRoad(this);
            targetIntersection.AddRoad(this);
        }

        private Lane[] InitLanes(int numLanes)
        {
            Lane[] lanes = new Lane[numLanes];
            for (int i = 0; i < numLanes; i++) lanes[i] = new Lane();
            return lanes;
        }

        /// <summary>
        /// We're clean bois, always clean up after yourself!
        /// </summary>
        ~Road()
        {
            sourceIntersection.RemoveRoad(this);
            targetIntersection.RemoveRoad(this);
        }

        public void ComputeLaneGeometry()
        {
            Segment[] srcSubSegment = sourceIntersection.GetRoadSegment(targetIntersection).SplitSegment(NumLanes, false);
            Segment[] targetSubSegment = targetIntersection.GetRoadSegment(sourceIntersection).SplitSegment(NumLanes, true);

            // Set the source segments of the sourth bound lanes: targetIntersection -> sourceIntersection (left)
            int i = 0;
            for (; i < numLanesSouthBound; i++)
            {
                SouthBoundLanes[i].SourceSegment = targetSubSegment[i];
                SouthBoundLanes[i].TargetSegment = srcSubSegment[i];
            }

            // Set the source segments of the north bound lanes: sourceIntersection -> targetIntersection (right)
            for (; i < NumLanes; i++)
            {
                NorthBoundLanes[i- numLanesSouthBound].SourceSegment = srcSubSegment[i];
                NorthBoundLanes[i- numLanesSouthBound].TargetSegment = targetSubSegment[i];
            }
        }

    }
}
