using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.IntersectionLogic;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    /// <summary>
    /// Represents an intersection. Supports up-to four incoming roads.
    /// </summary>
    class FourWayIntersection : IRTSUpdateable, IRTSGeometry<Rectangle>
    {
        /// <summary>
        /// Center of the intersection
        /// </summary>
        public Vector2 Origin { get; private set; }

        // Road information
        private static int MAX_ROADS = 4;
        public List<Road> Roads;

        private IntersectionFlowState currentIntersectionFlowState;
        private TrafficLightFSM[] trafficLightFSMs;

        /// <summary>
        /// Current traffic light state, controls the flow of traffic through the intersection
        /// </summary>
        public TrafficLightFSM CurrentTrafficLightState => trafficLightFSMs[(int)currentIntersectionFlowState];
        private int roadCount { get => Roads.Count; }

        /// <summary>
        /// Width of the intersection along the x-axis. Based on the roads connected to it and the
        /// number of lanes in them.
        /// </summary>
        public float Width
        {
            get
            {
                float maxHorizontalRoadWidth = Lane.LANE_WIDTH;
                foreach (Road r in Roads)
                {
                    if (!r.IsHorizontal) maxHorizontalRoadWidth = Math.Max(maxHorizontalRoadWidth, r.RoadWidth);
                }
                return maxHorizontalRoadWidth;
            }
        }

        /// <summary>
        /// Height of the intersection along the y-axis. Based on the roads connected to it and the
        /// number of lanes in them.
        /// </summary>
        public float Height
        {
            get
            {
                float maxVerticalRoadWidth = Lane.LANE_WIDTH;
                foreach (Road r in Roads)
                {
                    if (r.IsHorizontal) maxVerticalRoadWidth = Math.Max(maxVerticalRoadWidth, r.RoadWidth);
                }
                return maxVerticalRoadWidth;
            }
        }

        /// <summary>
        /// Creates an intersection
        /// </summary>
        /// <param name="origin">Center of the intersection</param>
        public FourWayIntersection(Vector2 origin)
        {
            Origin = origin;
            Roads = new List<Road>();
        }

        /// <summary>
        /// Next lane after a given lane. Depends on traffic light state. If null, vehicle must stop
        /// at the end of the lane
        /// </summary>
        /// <param name="lane">Incoming lane</param>
        /// <returns>Outgoing lane</returns>
        public Lane NextLane(Lane lane)
        {
            // Get currently possible lanes based on intersection lights. Null means red light
            List<Lane> possibleLanes = CurrentTrafficLightState.GetPossibleNextLanes(lane.TargetSegment);
            if (possibleLanes.Count > 0) return possibleLanes[new Random().Next(possibleLanes.Count)];
            else return null;
        }

        /// <summary>
        /// Adds road to intersection, if possible
        /// </summary>
        /// <param name="road">Road to add</param>
        public void AddRoad(Road road)
        {
            if (roadCount < MAX_ROADS)
            {
                Roads.Add(road);

                // Reconfigure the geometry since height and width may have changed
                ConfigureGeometry();
            }
            else throw new ArgumentOutOfRangeException(String.Format("Max number ({0}) roads already reached!", roadCount));
        }

        /// <summary>
        /// Remove a road from the intersection
        /// </summary>
        /// <param name="road"></param>
        public void RemoveRoad(Road road)
        {
            bool ok = Roads.Remove(road);
                // Reconfigure the geometry since height and width may have changed
            if (ok) ConfigureGeometry();
        }

        /// <summary>
        /// Configure's the geometry of an intersection. Also triggers the geometry computation
        /// in all the connected road.
        /// </summary>
        private void ConfigureGeometry()
        {
            // Compute Road geometry -> i.e lane placement as they rely on the road length
            // which in turn depends on the intersection width, which depends on the roads
            // So every time a road is added, we need to recompute the geometry of all the lanes
            // so all contraints are satifsied
            foreach (Road r in Roads) r.ComputeLaneGeometry();

            // Compute intersection lanes as these depend on the road geometry
            trafficLightFSMs = new TrafficLightFSM[] {
                new TrafficLightFSM(this, IntersectionFlowState.NS_FR, IntersectionFlowState.NS_L, 10), // NS_FR
                new TrafficLightFSM(this, IntersectionFlowState.NS_L, IntersectionFlowState.EW_FR, 10), // NS_L
                new TrafficLightFSM(this, IntersectionFlowState.EW_FR, IntersectionFlowState.EW_L, 10), // EW_FR
                new TrafficLightFSM(this, IntersectionFlowState.EW_L, IntersectionFlowState.NS_FR, 10)  // EW_L
            };

            int laneIdx = 0;
            foreach (Road r in Roads)
            {
                foreach (Road s in Roads)
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

        /// <summary>
        /// Dimensions of the intersection
        /// </summary>
        public Vector2 Dimensions => new Vector2(Width, Height);

        /// <summary>
        /// Get the side segment of the intersection corresonding to the road that
        /// heads towards another intersection.
        /// </summary>
        /// <param name="other">Other intersection</param>
        /// <param name="roadWidth">Width of the road (may be smaller than that of the intersection)</param>
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

        /// <summary>
        /// Update traffic light state
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Update the active lanes (cars need the leader info for IDM and MOBIL)
            foreach(List<Lane> ll in CurrentTrafficLightState.activeLanes.Values) 
                foreach(Lane l in ll) 
                    l.Update(deltaTime);

            // Update light controllers
            IntersectionFlowState nextIntersectionFlowState = CurrentTrafficLightState.Update(deltaTime);
            if (nextIntersectionFlowState != currentIntersectionFlowState)
            {
                CurrentTrafficLightState.OnExit();
                currentIntersectionFlowState = nextIntersectionFlowState;
                CurrentTrafficLightState.OnEnter();
            }
        }

        /// <summary>
        /// Geometrical representation of an intersection
        /// </summary>
        /// <returns>Rectangle with intersections dimensions</returns>
        public Rectangle GetGeometricalFigure()
        {
            return new Rectangle(Origin, Width, Height);
        }
    }
}
