using System;
using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.DrivingLogic
{
    /// <summary>
    /// Helper class to compute acceleration based on Intelligent Driver Model
    /// Based on the following paper: Treiber, Martin; Hennecke, Ansgar; Helbing, 
    /// Dirk (2000), "Congested traffic states in empirical observations and microscopic simulations", 
    /// Physical Review E, 62 (2): 1805–1824
    /// </summary>
    class IntelligentDriverModel
    {
        public static float SAFE_TIME_HEADWAY = 4f;    // Minimum possible time to car in front (in seconds)
        public static float MIN_BUMPER_TO_BUMPER_DISTANCE = 2; // Minimum gap between cars (in meters)
        public static float MAX_BRAKING = 9;            // Maximum braking of a vehicle (in meters/seconds^2)
        public static float MIN_ACCELERATION = 0.2f;    // Minimum acceleration of a vehicle (in meters/seconds^2)

        /// <summary>
        /// Computes the target acceleration of car based on the Intelligent Driver Model
        /// </summary>
        /// <param name="curr">Car for which we want to compute the acceleration</param>
        /// <param name="next">Car in the lane in front of current car</param>
        /// <returns></returns>
        public static Vector2 ComputeAccelerationIntensity(Vehicle curr, Vector2 laneDirection, float distanceToNextCar, float approachingRate)
        {
            // Car info
            float currSpeed = Vector2.Dot(curr.LinearVelocity, laneDirection); // Car speed along direction of traffic

            float a = curr.MaxAcceleration;
            float b = curr.BrakingDeceleration;

            // Speed adjustement
            float accelerationExponent = 4;
            float vTerm = (float) Math.Pow(currSpeed / curr.MaxOverrallSpeed, accelerationExponent);

            // Gap adjustement
            float desiredGap =
                MIN_BUMPER_TO_BUMPER_DISTANCE +
                currSpeed * curr.HeadwayTime +
                currSpeed * approachingRate / (2 * (float) Math.Sqrt(a * b));
            float gapTerm = desiredGap / distanceToNextCar;

            float accelerationIntensity = a * (1 - vTerm - gapTerm);
            return accelerationIntensity * laneDirection;
        }

    }
}
