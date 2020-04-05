using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Physics;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    class Car : RigidBody
    {
        // TODO Add trajectory, some waypoints, etc
        private Lane lane;

        /// <summary>
        /// Creates a car with a mass and an initial position
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="position"></param>
        public Car(float mass, Lane lane, float timeOffset = 0) : base(mass, lane.Trajectory.GetPosition(timeOffset))
        {
            this.lane = lane;
        }

        /// <summary>
        /// Compute update using the Intelligent Driver Model for car-following model and MOBIL lane change criterion
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Reset car to start of lane
            if (lane.ReachedEndOfLane(Positon))
            {
                Positon = lane.Trajectory.GetPosition(0);
            }
            else
            {
                // Update forces acting on the car
                float idmAcc = 5; // Add IDM static class with actual calculations based on vehicle positon + lane state, etc
                float step = lane.GetProgression(Positon);
                Vector2 targetAcceleration = lane.Trajectory.GetTangent(step).Normalized * idmAcc;
                Vector2 deltaAcceleration = targetAcceleration - Acceleration;
                Vector2 deltaForce = deltaAcceleration * Mass;
                ApplyForce(deltaForce);
            }
        }
    }
}
