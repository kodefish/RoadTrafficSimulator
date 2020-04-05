using System;
using RoadTrafficSimulator.DataStructures;
using RoadTrafficSimulator.Simulator.Physics;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    class Car : RigidBody
    {
        // TODO Add trajectory, some waypoints, etc

        /// <summary>
        /// Creates a car with a mass and an initial position
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="position"></param>
        public Car(float mass, Vector2 position) : base(mass, position)
        {

        }

        /// <summary>
        /// Compute update using the Intelligent Driver Model for car-following model and MOBIL lane change criterion
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Update forces acting on the car
            Vector2 targetAcceleration = new Vector2(); // TODO
            Vector2 deltaAcceleration = targetAcceleration - Acceleration;
            Vector2 deltaForce = deltaAcceleration * Mass;
            ApplyForce(deltaForce);
        }
}
