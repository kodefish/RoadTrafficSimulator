using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.WorldEntities;
using System;

namespace RoadTrafficSimulator.Simulator.IntersectionLogic
{
    /// <summary>
    /// Light states. Go is green, wait_for_empty is yellow, 
    /// and red is whenever the TrafficLightFSM state is not the current state.
    /// So when the traffic light FSM is active, cars may go through
    /// </summary>
    enum LightStates { GO, WAIT_FOR_EMPTY };

    /// <summary>
    /// Traffic light state. Represents a certain set of active lanes being active in an intersection.
    /// The state monitors the possible states and waits for the intersection to be empty before
    /// letting a new state take over.
    /// </summary>
    class TrafficLightFSM
    {
        /// <summary>
        /// Intersection to control
        /// </summary>
        private readonly FourWayIntersection intersection;

        /// <summary>
        /// Current and next intersection flow state
        /// </summary>
        private readonly IntersectionFlowState currentIntersectionFlowState, nextIntersectionFlowState;

        /// <summary>
        /// Mapping of entry segments (lane targets) to a list of possible next lanes
        /// (some lanes may turn right and go forward)
        /// </summary>
        public readonly Dictionary<int, List<Lane>> activeLanes;

        /// <summary>
        /// Duration, in seconds, of a green light
        /// </summary>
        private readonly float greenLightTime;

        /// <summary>
        /// Time spent in green light state
        /// </summary>
        private float counter;

        /// <summary>
        /// Current light state
        /// </summary>
        private LightStates currentLightState;

        /// <summary>
        /// Creates a traffic light state
        /// </summary>
        /// <param name="intersection">Intersection to control</param>
        /// <param name="currentIntersectionFlowState">Intersection flow state of this state</param>
        /// <param name="nextIntersectionFlowState">Intersection flow state to transition to</param>
        /// <param name="greenLightTime">Time to spend in green light</param>
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

        /// <summary>
        /// Add an active lane. A lane target can thus be used to determine the next lane
        /// </summary>
        /// <param name="key">Source segment</param>
        /// <param name="l">Lane starting at key</param>
        public void AddActiveLane(Segment key, Lane l) 
        {
            if (!activeLanes.ContainsKey(key.GetHashCode())) activeLanes.Add(key.GetHashCode(), new List<Lane>());
            activeLanes[key.GetHashCode()].Add(l);
        }

        /// <summary>
        /// Get a list of possible next lanes in given state starting at some segment
        /// </summary>
        /// <param name="s">Start segment</param>
        /// <returns>Possible lanes starting at s</returns>
        public List<Lane> GetPossibleNextLanes(Segment s)
        {
            // Only let cars go through if the current light state is go and there are available lanes
            if (currentLightState == LightStates.GO && activeLanes.ContainsKey(s.GetHashCode())) return activeLanes[s.GetHashCode()];
            else return new List<Lane>();
        }

        /// <summary>
        /// Clear the active lanes.
        /// </summary>
        public void Clear() => activeLanes.Clear();

        /// <summary>
        /// Update the traffic light FSM.
        /// </summary>
        /// <param name="deltaTime">Time since the last update</param>
        /// <returns></returns>
        public IntersectionFlowState Update(float deltaTime)
        {
            // Update counter
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

        /// <summary>
        /// Checks to see if all the currently active lanes are empty (no cars)
        /// </summary>
        /// <returns>True if all active lanes are empty</returns>
        private bool ActiveLanesEmpty()
        {
            bool isEmpty = true;
            foreach (List<Lane> ll in activeLanes.Values)
                foreach(Lane l in ll)
                    isEmpty = isEmpty && l.Cars.Count == 0;
            return isEmpty;
        }

        /// <summary>
        /// On Enter traffic light state. Reset the counter to zero and light to green
        /// </summary>
        public void OnEnter()
        {
            counter = 0;
            currentLightState = LightStates.GO;
        }

        /// <summary>
        /// On Exit traffic light state. Reset light to green
        /// </summary>
        public void OnExit()
        {
            currentLightState = LightStates.GO;
        }
    }
}