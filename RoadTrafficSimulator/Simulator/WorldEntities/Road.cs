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
        public int NumInLanes { get; }
        public int NumOutLanes { get; }
        public int NumLanes => NumInLanes + NumOutLanes;

        public Lane[] InLanes { get; private set; }
        public Lane[] OutLanes { get; private set; }

        // Road orientation
        public RoadOrientation Orientation { get; }
        public bool IsHorizontal => Orientation == RoadOrientation.Horizontal;

        // Road position
        public Segment RoadStartSegment => SourceIntersection.GetRoadSegment(TargetIntersection, RoadWidth);
        public Segment RoadTargetSegment => TargetIntersection.GetRoadSegment(SourceIntersection, RoadWidth);
        public Segment RoadMidline {
            get
            {
                Vector2 sepSrc = RoadStartSegment.Lerp(NumInLanes * Lane.LANE_WIDTH / RoadStartSegment.Length);
                Vector2 sepDst = RoadTargetSegment.Lerp(NumOutLanes * Lane.LANE_WIDTH/ RoadTargetSegment.Length);
                return new Segment(sepSrc, sepDst);
            }
        }
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
        /// <param name="numInLanes">Numlanes along negative orientation</param>
        /// <param name="numOutLanes">Numlanes along positive orientation</param>
        /// <param name="orientation"></param>
        public Road(
            ref FourWayIntersection source, ref FourWayIntersection target, 
            int numInLanes, int numOutLanes, 
            RoadOrientation orientation, float speedLimit)
        {
            Orientation = orientation;
            this.speedLimit = speedLimit;

            // Make sure road points along positive X or Y axis, and are aligned along the orientation
            // Not sure we still really need this bit ahahahahaha
            /*
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
            */
            SourceIntersection = source;
            TargetIntersection = target;

            this.NumInLanes = numInLanes;
            this.NumOutLanes = numOutLanes;
            InLanes = InitLanes(numInLanes);
            OutLanes = InitLanes(numOutLanes);

            SourceIntersection.AddRoad(this);
            TargetIntersection.AddRoad(this);
        }

        private Lane[] InitLanes(int numLanes)
        {
            Lane[] lanes = new Lane[numLanes];
            for (int i = 0; i < numLanes; i++) lanes[i] = new Lane(i, speedLimit);
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
            Segment outLanesSources = new Segment(RoadStartSegment.Source, RoadMidline.Source);
            Segment outLanesTargets = new Segment(RoadTargetSegment.Target, RoadMidline.Target);
            SetLaneSourceAndTargets(OutLanes, outLanesSources, outLanesTargets);

            // Out lanes go from top half of source segment, to bottom half of target segment
            Segment inLanesSources = new Segment(RoadTargetSegment.Source, RoadMidline.Target);
            Segment inLanesTargets = new Segment(RoadStartSegment.Target, RoadMidline.Source);
            SetLaneSourceAndTargets(InLanes, inLanesSources, inLanesTargets);
        }

        private void SetLaneSourceAndTargets(Lane[] lanes, Segment sourceSegment, Segment targetSegment)
        {
            int numLanes = lanes.Length;
            // Reverse target segments
            Segment[] sourceSubSegment = sourceSegment.SplitSegment(numLanes);
            Segment[] targetSubSegment = targetSegment.SplitSegment(numLanes);

            for (int i = 0; i < lanes.Length; i++)
            {
                lanes[i].SourceSegment = sourceSubSegment[i];
                lanes[i].TargetSegment = targetSubSegment[i];
            }
        }

        public void Update(float deltaTime)
        {
            // Update all the lanes with new car information
            foreach (Lane l in InLanes) l.Update(deltaTime);
            foreach (Lane l in OutLanes) l.Update(deltaTime);
        }

        public Rectangle GetGeometricalFigure()
        {
            if (IsHorizontal)
                return new Rectangle(Position, RoadWidth, RoadLength, (float)Math.PI / 2);
            else 
                return new Rectangle(Position, RoadWidth, RoadLength, 0);
        }
    }
}
