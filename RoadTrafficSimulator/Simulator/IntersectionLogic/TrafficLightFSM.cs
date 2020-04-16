using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.WorldEntities;
using System;

namespace RoadTrafficSimulator.Simulator.IntersectionLogic
{
    enum LightStates { GO, WAIT_FOR_EMPTY };
    class TrafficLightFSM
    {
        private readonly FourWayIntersection intersection;
        private readonly IntersectionFlowState currentIntersectionFlowState, nextIntersectionFlowState;
        public readonly Dictionary<int, List<Lane>> activeLanes;
        private readonly float greenLightTime;

        private float counter;
        private LightStates currentLightState;

        public TrafficLightFSM(
            FourWayIntersection intersection, 
            IntersectionFlowState currentIntersectionFlowState, 
            IntersectionFlowState nextIntersectionFlowState, 
            float greenLightTime
        ) {
            this.intersection = intersection;
            this.currentIntersectionFlowState = currentIntersectionFlowState;
            this.nextIntersectionFlowState = nextIntersectionFlowState;
            this.greenLightTime = greenLightTime;
            activeLanes = new Dictionary<int, List<Lane>>();
        }

        public void AddActiveLane(Segment key, Lane l) 
        {
            if (!activeLanes.ContainsKey(key.GetHashCode())) activeLanes.Add(key.GetHashCode(), new List<Lane>());
            activeLanes[key.GetHashCode()].Add(l);
        }

        public List<Lane> GetPossibleNextLanes(Segment s)
        {
            // Only let cars go through if the current light state is go and there are available lanes
            if (currentLightState == LightStates.GO)
            {
                try { 
                    List<Lane> result = activeLanes[s.GetHashCode()];
                    return result; }
                catch (KeyNotFoundException) { return new List<Lane>(); }
            }
            else return new List<Lane>();
        }

        public void Clear() => activeLanes.Clear();

        public IntersectionFlowState Update(float deltaTime)
        {
            counter += deltaTime;
            if (currentLightState == LightStates.WAIT_FOR_EMPTY)
            {
                // Wait for the intersection to be empty before moving to next state
                if (ActiveLanesEmpty()) return nextIntersectionFlowState;
            }
            else
            {
                // Check counter and transition to waiting for an empty intersection if 
                // time's up
                if (counter > greenLightTime) currentLightState = LightStates.WAIT_FOR_EMPTY;
            }
            return currentIntersectionFlowState;
        }

        private bool ActiveLanesEmpty()
        {
            bool isEmpty = true;
            foreach (List<Lane> ll in activeLanes.Values)
                foreach(Lane l in ll)
                    isEmpty = isEmpty && l.Cars.Count == 0;
            return isEmpty;
        }

        public void OnEnter()
        {
            counter = 0;
            currentLightState = LightStates.GO;
        }
        public void OnExit()
        {
            currentLightState = LightStates.GO;
        }
    }
}