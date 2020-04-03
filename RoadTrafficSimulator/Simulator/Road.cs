using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadTrafficSimulator.DataStructures;
using RoadTrafficSimulator.Interfaces;

namespace RoadTrafficSimulator.Simulator
{
    enum RoadOrientation
    {
        Vertical, Horizontal
    }

    class Road : IRTSDimension, IRTSPosition
    {
        // TODO: Move this to Lane class
        public const float LANE_WIDTH = 2;    // Width of a lane, in meters

        // Code contract : source.Origin.X < target.Origin.X for horizontal streets
        // Code contract : source.Origin.Y < target.Origin.Y for vertical streets
        private FourWayIntersection sourceIntersection, targetIntersection;
        private readonly int numLanesSouthbound;
        private readonly int numLanesNorthBound;

        // Road orientation
        public RoadOrientation Orientation { get; }
        public bool IsHorizontal => Orientation == RoadOrientation.Horizontal;

        // Road dimensions
        public float RoadWidth => numLanesSouthbound * LANE_WIDTH + numLanesNorthBound * LANE_WIDTH;
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
        /// <param name="numLanesSouthbound">Numlanes along negative orientation</param>
        /// <param name="numLanesNorthBound">Numlanes along positive orientation</param>
        /// <param name="orientation"></param>
        public Road(
            FourWayIntersection source, FourWayIntersection target, 
            int numLanesSouthbound, int numLanesNorthBound, 
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

            this.numLanesSouthbound = numLanesSouthbound;
            this.numLanesNorthBound = numLanesNorthBound;

            sourceIntersection.AddRoad(this);
            targetIntersection.AddRoad(this);
        }

        /// <summary>
        /// We're clean bois, always clean up after yourself!
        /// </summary>
        ~Road()
        {
            sourceIntersection.RemoveRoad(this);
            targetIntersection.RemoveRoad(this);
        }

        public Vector2 GetDimensions()
        {
            if (Orientation == RoadOrientation.Vertical) return new Vector2(RoadWidth, RoadLength);
            else  return new Vector2(RoadLength, RoadWidth);
        }

        public Vector2 GetGlobalPosition()
        {
            return (sourceIntersection.Origin + targetIntersection.Origin) / 2;
        }
    }
}
