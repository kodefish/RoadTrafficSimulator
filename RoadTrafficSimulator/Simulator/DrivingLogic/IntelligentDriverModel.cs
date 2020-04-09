using System;
using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.DrivingLogic
{
    class IntelligentDriverModel
    {
        public static float SAFE_TIME_HEADWAY = 4f;    // Minimum possible time to car in front (in seconds)
        public static float MIN_BUMPER_TO_BUMPER_DISTANCE = 4; // Minimum gap between cars (in meters)

        /// <summary>
        /// Computes the target acceleration of car based on the Intelligent Driver Model
        /// </summary>
        /// <param name="curr">Car for which we want to compute the acceleration</param>
        /// <param name="next">Car in the lane in front of current car</param>
        /// <returns></returns>
        public static Vector2 ComputeAccelerationIntensity(Car curr, Vector2 laneDirection, float distanceToNextCar, float approachingRate)
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
                currSpeed * SAFE_TIME_HEADWAY +
                currSpeed * approachingRate / (2 * (float) Math.Sqrt(a * b));
            float gapTerm = desiredGap / distanceToNextCar;

            float accelerationIntensity = a * (1 - vTerm - gapTerm);
            return laneDirection * accelerationIntensity;
        }

    }
}
