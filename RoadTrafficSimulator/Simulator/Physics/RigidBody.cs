using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.Physics
{
    /// <summary>
    /// Rigid body with a position, velocity, force and mass. Can apply some force on it to move it in some desired direction
    /// </summary>
    abstract class RigidBody
    {
        public float Mass { get; private set; }                         // In [kg]
        public Vector2 Position { get; set; }                    // In [meters]
        public Vector2 Velocity { get; private set; }                   // In [meters/second]
        public Vector2 Force { get; private set; }                      // In [Newtons]
        public Vector2 Acceleration { get { return Force / Mass; } }    // In [meters/second^2]

        /// <summary>
        /// Create a rigid body with mass, initial position, initial velocity, and initial force
        /// </summary>
        /// <param name="mass">Mass of the rigid body</param>
        /// <param name="position">Initial position of the rigid body</param>
        /// <param name="velocity">Initial velocity of the rigid body</param>
        /// <param name="force">Inital force applied to the rigid body</param>
        public RigidBody(float mass, Vector2 position, Vector2 velocity, Vector2 force)
        {
            Mass = mass;
            Position = position;
            Velocity = velocity;
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
        /// <param name="force">Force to apply, in Newtons</param>
        public void ApplyForce(Vector2 force) => Force += force;

        /// <summary>
        /// Apply the forces to the rigid body, update position and velocity given current force
        /// </summary>
        /// <param name="deltaTime">Time step over which to apply the force</param>
        public void IntegrateForces(float deltaTime)
        {
            float deltaTime2 = deltaTime * deltaTime;

            // p = p + v*t + 0.5 * a * t^2 (basic physics)
            Position = Position + Velocity * deltaTime + Acceleration * 0.5f * deltaTime2;

            // v = v + a * t
            Velocity = Velocity + Acceleration * deltaTime;
        }

    }
}
