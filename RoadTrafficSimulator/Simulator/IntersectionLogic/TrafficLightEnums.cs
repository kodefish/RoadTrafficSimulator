using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.IntersectionLogic
{
    /// <summary>
    /// Represents a cardinal direction (north, east, south, west)
    /// </summary>
    enum CardinalDirection { NORTH, EAST, SOUTH, WEST }

    /// <summary>
    /// Possible turn directions (front, right, left)
    /// </summary>
    enum TurnDirection { FRONT, RIGHT, LEFT }

    /// <summary>
    /// Intersection traffic flow (vertical/horizontal -> NS/EW and front + right / left)
    /// </summary>
    enum IntersectionFlowState { NS_FR, NS_L, EW_FR, EW_L }

    class TrafficEnum
    {
        /// <summary>
        /// Compute cardinal direction of a vector
        /// </summary>
        public static CardinalDirection GetCardinalDirection(Vector2 a)
        {
            float xDot = Vector2.Dot(a, Vector2.UnitX);
            float yDot = Vector2.Dot(a, Vector2.UnitY);
            if (xDot > 0 && yDot == 0) return CardinalDirection.EAST;
            if (xDot < 0 && yDot == 0) return CardinalDirection.WEST;
            if (xDot == 0 && yDot > 0) return CardinalDirection.NORTH;
            if (xDot == 0 && yDot < 0) return CardinalDirection.SOUTH;

            throw new ArgumentException("Vector must be aligned to x or y axis");
        }

        /// <summary>
        /// Compute the relative turn direction between two vectors
        /// </summary>
        /// <param name="inDir">Direction coming into the turn</param>
        /// <param name="outDir">Direction coming out of the turn</param>
        public static TurnDirection GetTurnDirection(Vector2 inDir, Vector2 outDir)
        {
            float turnSign = inDir.X * outDir.Y - outDir.X * inDir.Y;
            if (turnSign > 0) return TurnDirection.RIGHT;
            else if (turnSign < 0) return TurnDirection.LEFT;
            else return TurnDirection.FRONT;
        }

        /// <summary>
        /// Get intersection traffic flow state (corresponding light basically) 
        /// based on incoming directoin and relative turn direction
        /// </summary>
        /// <param name="inDirection">Direction coming into the intersection</param>
        /// <param name="turnDirection">Relative turn direction</param>
        public static IntersectionFlowState GetStateIdx(CardinalDirection inDirection, TurnDirection turnDirection)
        {
            // God I miss Scala and pattern matching...
            if (inDirection == CardinalDirection.NORTH && turnDirection == TurnDirection.FRONT) return IntersectionFlowState.NS_FR;
            if (inDirection == CardinalDirection.NORTH && turnDirection == TurnDirection.RIGHT) return IntersectionFlowState.NS_FR;
            if (inDirection == CardinalDirection.NORTH && turnDirection == TurnDirection.LEFT) return IntersectionFlowState.NS_L;

            if (inDirection == CardinalDirection.EAST && turnDirection == TurnDirection.FRONT) return IntersectionFlowState.EW_FR;
            if (inDirection == CardinalDirection.EAST && turnDirection == TurnDirection.RIGHT) return IntersectionFlowState.EW_FR;
            if (inDirection == CardinalDirection.EAST && turnDirection == TurnDirection.LEFT) return IntersectionFlowState.EW_L;

            if (inDirection == CardinalDirection.SOUTH && turnDirection == TurnDirection.FRONT) return IntersectionFlowState.NS_FR;
            if (inDirection == CardinalDirection.SOUTH && turnDirection == TurnDirection.RIGHT) return IntersectionFlowState.NS_FR;
            if (inDirection == CardinalDirection.SOUTH && turnDirection == TurnDirection.LEFT) return IntersectionFlowState.NS_L;

            if (inDirection == CardinalDirection.WEST && turnDirection == TurnDirection.FRONT) return IntersectionFlowState.EW_FR;
            if (inDirection == CardinalDirection.WEST && turnDirection == TurnDirection.RIGHT) return IntersectionFlowState.EW_FR;
            if (inDirection == CardinalDirection.WEST && turnDirection == TurnDirection.LEFT) return IntersectionFlowState.EW_L;

            throw new ArgumentException("Illegal turn combination!");
        }
    }
}