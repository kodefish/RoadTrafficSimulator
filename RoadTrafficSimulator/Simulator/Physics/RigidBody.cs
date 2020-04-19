using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.Physics
{
    /// <summary>
    /// Rigid body with a position, velocity, force and mass. Can apply some force on it to move it in some desired direction
    /// </summary>
    abstract class RigidBody
    {
        /// <summary>
        /// Mass of the rigid body, in [kg]
        /// </summary>
        public float Mass { get; private set; }

        /// <summary>
        /// Position of the rigid body, in [meters]
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Linear velocity of the rigid body, in in [meters/second]
        /// </summary>
        public Vector2 LinearVelocity { get; private set; }

        /// <summary>
        /// Force applied to the rigid body, in [Newtons]
        /// </summary>
        public Vector2 Force { get; private set; }

        /// <summary>
        /// Acceleration of the rigid body, in [meters/second^2]
        /// </summary>
        public Vector2 Acceleration { get { return Force / Mass; } }

        /// <summary>
        /// Create a rigid body with mass, initial position, initial velocity, and initial force
        /// </summary>
        /// <param name="mass">Mass of the rigid body</param>
        /// <param name="position">Initial position of the rigid body</param>
        /// <param name="linearVelocity">Initial velocity of the rigid body</param>
        /// <param name="force">Inital force applied to the rigid body</param>
        public RigidBody(
            float mass, Vector2 position, Vector2 linearVelocity, Vector2 force)
        {
            if (mass <= 0) throw new ArgumentException(String.Format("Mass: {0} must be greater than 0!", mass));

            Mass = mass;
            Position = position;
            LinearVelocity = linearVelocity;
            Force = force;
        }

        /// <summary>
        /// Create a rigid body with mass, initial position, and initial velocity
        /// </summary>
        /// <param name="mass">Mass of the rigid body</param>
        /// <param name="position">Initial position of the rigid body</param>
        /// <param name="velocity">Initial velocity of the rigid body</param>
        public RigidBody(float mass, Vector2 position, Vector2 velocity) : this(mass, position, velocity, new Vector2()) { }

        /// <summary>
        /// Create a rigid body with mass and initial position
        /// </summary>
        /// <param name="mass">Mass of the rigid body</param>
        /// <param name="position">Initial position of the rigid body</param>
        public RigidBody(float mass, Vector2 position) : this(mass, position, new Vector2(), new Vector2()) { }

        /// <summary>
        /// Apply an additional force to the rigid body
        /// </summary>
        /// <param name="force">delta force</param>
        public void ApplyForce(Vector2 force) => Force += force;

        /// <summary>
        /// Apply the forces to the rigid body, update position and velocity given current force
        /// </summary>
        /// <param name="deltaTime">Time step over which to apply the force</param>
        public void IntegrateForces(float deltaTime)
        {
            float deltaTime2 = deltaTime * deltaTime;

            // Update velocity and position
            LinearVelocity += Acceleration * deltaTime;
            Position += LinearVelocity * deltaTime;

            // Reset force to 0
            Force = new Vector2(0, 0);
        }
    }
}
