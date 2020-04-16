using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.IntersectionLogic;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    /// <summary>
    /// Supports up-to four incoming roads 
    /// </summary>
    class FourWayIntersection : IRTSUpdateable, IRTSGeometry<Rectangle>
    {
        // Intersection information
        public Vector2 Origin { get; private set; }

        // Road information
        private static int MAX_ROADS = 4;
        private List<Road> roads;

        private IntersectionFlowState currentIntersectionFlowState;
        private TrafficLightFSM[] trafficLightFSMs;
        public TrafficLightFSM CurrentTrafficLightState => trafficLightFSMs[(int)currentIntersectionFlowState];
        private int roadCount { get => roads.Count; }

        public float Width
        {
            get
            {
                float maxHorizontalRoadWidth = Lane.LANE_WIDTH;
                foreach (Road r in roads)
                {
                    if (!r.IsHorizontal) maxHorizontalRoadWidth = Math.Max(maxHorizontalRoadWidth, r.RoadWidth);
                }
                return maxHorizontalRoadWidth;
            }
        }
        public float Height
        {
            get
            {
                float maxVerticalRoadWidth = Lane.LANE_WIDTH;
                foreach (Road r in roads)
                {
                    if (r.IsHorizontal) maxVerticalRoadWidth = Math.Max(maxVerticalRoadWidth, r.RoadWidth);
                }
                return maxVerticalRoadWidth;
            }
        }

        public FourWayIntersection(Vector2 origin)
        {
            Origin = origin;
            roads = new List<Road>();
        }

        internal Lane NextLane(Lane lane)
        {
            // Get currently possible lanes based on intersection lights. Null means red light
            List<Lane> possibleLanes = CurrentTrafficLightState.GetPossibleNextLanes(lane.TargetSegment);
            if (possibleLanes.Count > 0) return possibleLanes[new Random().Next(possibleLanes.Count)];
            else return null;
        }

        // Adds road to intersection, if possible
        public void AddRoad(Road road)
        {
            if (roadCount < MAX_ROADS)
            {
                roads.Add(road);
                ConfigureLanes();
            }
            else throw new ArgumentOutOfRangeException(String.Format("Max number ({0}) roads already reached!", roadCount));
        }

        internal bool IsEmpty()
        {
            bool isEmpty = true;
            foreach (TrafficLightFSM state in trafficLightFSMs)
            {
                foreach (List<Lane> ll in state.activeLanes.Values)
                {
                    foreach (Lane l in ll) isEmpty = isEmpty && l.Cars.Count == 0;
                }
            }
            return isEmpty;
        }

        public void RemoveRoad(Road road)
        {
            bool ok = roads.Remove(road);
            if (ok) ConfigureLanes();
        }

        private void ConfigureLanes()
        {
            // Compute Road geometry -> i.e lane placement as they rely on the road length
            // which in turn depends on the intersection width, which depends on the roads
            // So every time a road is added, we need to recompute the geometry of all the lanes
            // so all contraints are satifsied
            foreach (Road r in roads) r.ComputeLaneGeometry();

            // Compute intersection lanes as these depend on the road geometry
            trafficLightFSMs = new TrafficLightFSM[] {
                new TrafficLightFSM(this, IntersectionFlowState.NS_FR, IntersectionFlowState.NS_L, 3), // NS_FR
                new TrafficLightFSM(this, IntersectionFlowState.NS_L, IntersectionFlowState.EW_FR, 3), // NS_L
                new TrafficLightFSM(this, IntersectionFlowState.EW_FR, IntersectionFlowState.EW_L, 3), // EW_FR
                new TrafficLightFSM(this, IntersectionFlowState.EW_L, IntersectionFlowState.NS_FR, 3), // EW_L
            };

            int laneIdx = 0;
            foreach (Road r in roads)
            {
                foreach (Road s in roads)
                {
                    if (r == s) continue; // foreach road s that is not r
                    // Get the lanes coming into the intersection along road r - depends on road orientation
                    // If the current intersection is the source intersection of the road, then the incoming lanes
                    // will be the incoming lanes of the road, and vice-versa
                    Lane[] incomingLanes = r.TargetIntersection == this ? r.OutLanes : r.InLanes;
                    Segment incomingTargetSegment = r.TargetIntersection == this ? r.OutLanesTargetSegment : r.InLanesTargetSegment;

                    // Get the lanes going out of the intersection along road s
                    Lane[] outgoingLanes = s.SourceIntersection == this ? s.OutLanes : s.InLanes;
                    Segment outgoingSourceSegment = s.SourceIntersection == this ? s.OutLanesSourceSegment : s.InLanesSourceSegment;

                    if (incomingLanes.Length > 0 && outgoingLanes.Length > 0)
                    {
                        // Determine turn direction (source: https://math.stackexchange.com/questions/555198/find-direction-of-angle-between-2-vectors)
                        Vector2 incomingDirection = incomingTargetSegment.Direction.Normal;
                        Vector2 outgoingDirection = outgoingSourceSegment.Direction.Normal;

                        CardinalDirection cardinalDirection = TrafficEnum.GetCardinalDirection(incomingDirection);
                        TurnDirection turnDirection = TrafficEnum.GetTurnDirection(incomingDirection, outgoingDirection);
                        IntersectionFlowState lightStateIdx = TrafficEnum.GetStateIdx(cardinalDirection, turnDirection);
                        TrafficLightFSM state = trafficLightFSMs[(int)lightStateIdx];

                        Segment source, target; Lane nextLane;
                        Lane intersectionLane = null;
                        switch (turnDirection)
                        {
                            case TurnDirection.RIGHT:
                                source = incomingLanes[incomingLanes.Length - 1].TargetSegment;
                                target = outgoingLanes[outgoingLanes.Length - 1].SourceSegment;
                                nextLane = outgoingLanes[outgoingLanes.Length - 1];
                                intersectionLane = new Lane(laneIdx++, (r.SpeedLimit + s.SpeedLimit) / 2, nextLane);
                                intersectionLane.SourceSegment = source;
                                intersectionLane.TargetSegment = target;

                                state.AddActiveLane(source, intersectionLane);
                                break;
                            case TurnDirection.LEFT:
                                source = incomingLanes[0].TargetSegment;
                                target = outgoingLanes[0].SourceSegment;
                                nextLane = outgoingLanes[0];
                                intersectionLane = new Lane(laneIdx++, (r.SpeedLimit + s.SpeedLimit) / 2, nextLane);
                                intersectionLane.SourceSegment = source;
                                intersectionLane.TargetSegment = target;

                                state.AddActiveLane(source, intersectionLane);
                                break;
                            case TurnDirection.FRONT:
                                // Match up as many lanes as possible. If there is a mismatch, then the last lane will just redirect to the closest
                                // forward lane
                                // -----------
                                // -----------
                                // ---^
                                for (int i = 0; i < Math.Min(incomingLanes.Length, outgoingLanes.Length); i++)
                                {
                                    source = incomingLanes[i].TargetSegment;
                                    target = outgoingLanes[Math.Min(i, outgoingLanes.Length - 1)].SourceSegment;
                                    nextLane = outgoingLanes[Math.Min(i, outgoingLanes.Length - 1)];
                                    intersectionLane = new Lane(laneIdx++, (r.SpeedLimit + s.SpeedLimit) / 2, nextLane);
                                    intersectionLane.SourceSegment = source;
                                    intersectionLane.TargetSegment = target;

                                    state.AddActiveLane(source, intersectionLane);
                                }

                                // Match up extra incoming lanes with last fully connected lane. 
                                // Cars will just automatically steer towards it and IDM will prevent them from crashing
                                for (int i = Math.Min(incomingLanes.Length, outgoingLanes.Length); i < incomingLanes.Length; i++)
                                {
                                    state.AddActiveLane(incomingLanes[i].TargetSegment, intersectionLane); 
                                }
                                break;
                            default: throw new Exception("Illegal traffic controller state!");
                        }
                    }
                }
            }
        }

        public Vector2 Dimensions => new Vector2(Width, Height);

        public Segment GetRoadSegment(FourWayIntersection other, float roadWidth)
        {
            Vector2 direction = (other.Origin - this.Origin).Normalized;
            Vector2 source = new Vector2(), target = new Vector2();
            if (direction.Equals(Vector2.Up))
            {
                source = new Vector2(Origin.X - roadWidth / 2, Origin.Y + Height / 2);
                target = new Vector2(Origin.X + roadWidth / 2, Origin.Y + Height / 2);
            }
            else if (direction.Equals(Vector2.Down))
            {
                source = new Vector2(Origin.X + roadWidth / 2, Origin.Y - Height / 2);
                target = new Vector2(Origin.X - roadWidth / 2, Origin.Y - Height / 2);
            }
            else if (direction.Equals(Vector2.Left))
            {
                source = new Vector2(Origin.X - Width / 2, Origin.Y - roadWidth / 2);
                target = new Vector2(Origin.X - Width / 2, Origin.Y + roadWidth / 2);
            }
            else if (direction.Equals(Vector2.Right))
            {
                source = new Vector2(Origin.X + Width / 2, Origin.Y + roadWidth / 2);
                target = new Vector2(Origin.X + Width / 2, Origin.Y - roadWidth / 2);

            }

            return new Segment(source, target);
        }

        public void Update(float deltaTime)
        {
            // Update light controllers
            foreach(List<Lane> ll in CurrentTrafficLightState.activeLanes.Values) 
                foreach(Lane l in ll) 
                    l.Update(deltaTime);

            IntersectionFlowState nextIntersectionFlowState = CurrentTrafficLightState.Update(deltaTime);
            if (nextIntersectionFlowState != currentIntersectionFlowState)
            {
                CurrentTrafficLightState.OnExit();
                currentIntersectionFlowState = nextIntersectionFlowState;
                CurrentTrafficLightState.OnEnter();
            }
        }

        public Rectangle GetGeometricalFigure()
        {
            return new Rectangle(Origin, Width, Height);
        }
    }
}
