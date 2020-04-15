using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.IntersectionLogic
{
    enum CardinalDirection { NORTH, EAST, SOUTH, WEST }
    enum TurnDirection { FRONT, RIGHT, LEFT }
    enum IntersectionFlowState { NS_FR, NS_L, EW_FR, EW_L }

    class TrafficEnum
    {
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

        public static TurnDirection GetTurnDirection(Vector2 inDir, Vector2 outDir)
        {
            float turnSign = inDir.X * outDir.Y - outDir.X * inDir.Y;
            if (turnSign > 0) return TurnDirection.RIGHT;
            else if (turnSign < 0) return TurnDirection.LEFT;
            else return TurnDirection.FRONT;
        }
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