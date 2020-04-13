using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.Physics
{
    /// <summary>
    /// Rigid body with a position, velocity, force and mass. Can apply some force on it to move it in some desired direction
    /// </summary>
    abstract class RigidBody
    {
        public float Mass { get; private set; }                         // In [kg]
        public float MoI { get; private set; }                          // In [kg * m2]

        public Vector2 Position { get; set; }                           // In [meters]
        public float Angle { get; private set; }                        // Angle w.r.t X axis, in [radians]

        public Vector2 LinearVelocity { get; private set; }             // In [meters/second]
        public float AngularVelocity { get; private set; }              // In [radians / second]

        public Vector2 Force { get; private set; }                      // In [Newtons]
        public float Torque { get; private set; }                       // In [radians / second2]

        public Vector2 Acceleration { get { return Force / Mass; } }    // In [meters/second^2]

        // Direction faces along the y-axis of the rigid body
        public virtual Vector2 Direction {
            get {
                return new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle)).Normal;
            }
        }

        /// <summary>
        /// Create a rigid body with mass, initial position, initial velocity, and initial force
        /// </summary>
        /// <param name="mass">Mass of the rigid body</param>
        /// <param name="position">Initial position of the rigid body</param>
        /// <param name="linearVelocity">Initial velocity of the rigid body</param>
        /// <param name="force">Inital force applied to the rigid body</param>
        public RigidBody(
            float mass, float momentOfInertia,
            Vector2 position, Vector2 linearVelocity, Vector2 force,
            float angle, float angularVelocity, float torque)
        {
            if (mass <= 0 || momentOfInertia <= 0) throw new ArgumentException(
                String.Format("Mass: {0} and Moment of Inertia: {1} must be greater than 0!", mass, momentOfInertia));

            Mass = mass;
            MoI = momentOfInertia;

            Position = position;
            LinearVelocity = linearVelocity;
            Force = force;

            Angle = angle;
            AngularVelocity = angularVelocity;
            Torque = torque;
        }

        /// <summary>
        /// Create a rigid body with mass, initial position, and initial velocity
        /// </summary>
        /// <param name="mass">Mass of the rigid body</param>
        /// <param name="position">Initial position of the rigid body</param>
        /// <param name="velocity">Initial velocity of the rigid body</param>
        public RigidBody(float mass, float momentOfInertia, Vector2 position, float angle, Vector2 velocity) 
            : this(mass, momentOfInertia, position, velocity, new Vector2(), angle, 0, 0) { }

        /// <summary>
        /// Create a rigid body with mass and initial position
        /// </summary>
        /// <param name="mass">Mass of the rigid body</param>
        /// <param name="position">Initial position of the rigid body</param>
        public RigidBody(float mass, float momentOfInertia, Vector2 position, float angle) 
            : this(mass, momentOfInertia, position, new Vector2(), new Vector2(), angle, 0, 0) { }

        /// <summary>
        /// Apply an additional force to the rigid body
        /// </summary>
        /// <param name="force">delta force</param>
        public void ApplyForce(Vector2 force) => Force += force;

        /// <summary>
        /// Apply an additional torque to the rigid body
        /// </summary>
        /// <param name="torque">delta torque</param>
        public void ApplyTorque(float torque) => Torque += torque;

        /// <summary>
        /// Apply the forces to the rigid body, update position and velocity given current force
        /// </summary>
        /// <param name="deltaTime">Time step over which to apply the force</param>
        public void IntegrateForces(float deltaTime)
        {
            float deltaTime2 = deltaTime * deltaTime;

            // Update acceleration
            LinearVelocity += Acceleration * deltaTime;
            Position += LinearVelocity * deltaTime;

            float angularAcceleration = Torque / MoI;
            AngularVelocity += angularAcceleration * deltaTime;
            Angle += AngularVelocity * deltaTime;

            // Reset forces to 0
            Force = new Vector2(0, 0);
            Torque = 0;
        }

    }
}
