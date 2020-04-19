using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Physics;
using RoadTrafficSimulator.Simulator.DrivingLogic;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    /// <summary>
    /// Parametrisation of a vehicle. Includes information about the vehicle and driver
    /// </summary>
    struct VehicleParams
    {
        public VehicleParams(float mass,
            float vehicleWidth,
            float vehicleLength,
            float maxSpeed,
            float maxSteeringAngle,
            float maxAccleration,
            float brakingDeceleration,
            float politenessFactor,
            float headwayTime)
        {
            Mass = mass;
            VehicleWidth = vehicleWidth;
            VehicleLength = vehicleLength;
            MaxSpeed = maxSpeed;
            MaxSteeringAngle = maxSteeringAngle;
            MaxAccleration = maxAccleration;
            BrakingDeceleration = brakingDeceleration;
            PolitenessFactor = politenessFactor;
            HeadwayTime = headwayTime;
        }

        /// <summary>
        /// Mass of the vehicle
        /// </summary>
        public float Mass { get; }

        /// <summary>
        /// Vehicle width, in meters
        /// </summary>
        public float VehicleWidth { get; }

        /// <summary>
        /// Vehicle length, in meters
        /// </summary>
        public float VehicleLength { get; }

        /// <summary>
        /// Max speed of the vehicle, in m/s
        /// </summary>
        public float MaxSpeed { get; }

        /// <summary>
        /// Maximum steering angle, in radians
        /// </summary>
        public float MaxSteeringAngle { get; }

        /// <summary>
        /// Maximum acceleration, in m/s2
        /// </summary>
        public float MaxAccleration { get; }

        /// <summary>
        /// Braking deceleration, positive number, in m/s2
        /// </summary>
        public float BrakingDeceleration { get; }

        /// <summary>
        /// Politeness factor of the driver. 0 -> egoistic, 1 -> altruistic
        /// </summary>
        public float PolitenessFactor { get; }

        /// <summary>
        /// Time to vehicle in front, driving schools recommend 2s
        /// </summary>
        public float HeadwayTime { get; }

        /// <summary>
        /// Paramerters characterising a car
        /// </summary>
        public static VehicleParams Car => new VehicleParams(
                    mass : 500,
                    vehicleWidth : 2,
                    vehicleLength : 3,
                    maxSpeed : 120,
                    maxSteeringAngle: (float)Math.PI / 6,
                    maxAccleration : 1.3f,
                    brakingDeceleration: 3f,
                    politenessFactor: 0.3f,
                    headwayTime: 4f
                );

        /// <summary>
        /// Paramerters characterising a truck
        /// </summary>
        public static VehicleParams Truck => new VehicleParams(
                    mass : 5000,
                    vehicleWidth : 2,
                    vehicleLength : 7,
                    maxSpeed : 80,
                    maxSteeringAngle: (float)Math.PI / 6,
                    maxAccleration : 0.3f,
                    brakingDeceleration: 2f,
                    politenessFactor: 0.7f,
                    headwayTime: 8f
                );
    }

    /// <summary>
    /// Class representing a vehicle 
    /// </summary>
    class Vehicle : RigidBody, IRTSGeometry<Rectangle>
    {
        private readonly int vehicleIdx;

        // Car params
        private readonly VehicleParams vehicleParams;

        // Acceleration constant params
        public float VehicleWidth => vehicleParams.VehicleWidth;
        public float VehicleLength => vehicleParams.VehicleLength;
        public float MaxVehicleSpeed => vehicleParams.MaxSpeed;
        public float MaxSteeringAngle => vehicleParams.MaxSteeringAngle;
        public float MaxAcceleration => vehicleParams.MaxAccleration;
        public float BrakingDeceleration => vehicleParams.BrakingDeceleration;
        public float PolitnessFactor => vehicleParams.PolitenessFactor;
        public float HeadwayTime => vehicleParams.HeadwayTime;

        // AI Finite state machine
        public DrivingState DrivingState { get; private set; }
        public float MaxOverrallSpeed => DrivingState.MaxSpeed();

        // Physical properties
        // Angle  (Velocity is along the y axis of the car -> angle is Angle of linear velocity - 90°)
        private float _angle;

        /// <summary>
        /// Returns the last angle when car was moving
        /// </summary>
        public float Angle {
            get {
                if (LinearVelocity.Norm > 0)
                {
                    _angle = LinearVelocity.Angle - (float) Math.PI / 2;
                }
                return _angle;
            }
        }

        // Direction (aligned with linear velocity)
        private Vector2 _direction;

        /// <summary>
        /// Returns the last direction when car was moving
        /// </summary>
        public Vector2 Direction {
            get {
                if (LinearVelocity.Norm > 0)
                {
                    _direction = LinearVelocity.Normalized;
                }
                return _direction;
            }
        }

        /// <summary>
        /// Creates a car with a mass and an initial position
        /// </summary>
        /// <param name="vehicleParams">Description of the car</param>
        /// <param name="initialLane">Starting lane</param>
        /// <param name="lerpOffset">Offset along the lane trajectory</param>
        public Vehicle(int vehicleIdx, VehicleParams vehicleParams, Lane initialLane, float lerpOffset = 0)
            : base(
                vehicleParams.Mass, 
                initialLane.Path.Lerp(lerpOffset))
        {
            // Make sure vehicle respects min and max acceleration params
            if (vehicleParams.MaxAccleration < IntelligentDriverModel.MIN_ACCELERATION) 
                throw new ArgumentException(String.Format("Car acceleration ({0} m/s2) too low! Min: {1} m/s2", vehicleParams.MaxAccleration, IntelligentDriverModel.MIN_ACCELERATION));
            if (vehicleParams.BrakingDeceleration > IntelligentDriverModel.MAX_BRAKING) 
                throw new ArgumentException(String.Format("Car braking ({0} m/s2) too high! Max: {1} m/s2", vehicleParams.MaxAccleration, IntelligentDriverModel.MAX_BRAKING));
            if (vehicleParams.HeadwayTime < IntelligentDriverModel.SAFE_TIME_HEADWAY) 
                throw new ArgumentException(String.Format("Headway time ({0} s) too low! Min: {1} s", vehicleParams.MaxAccleration, IntelligentDriverModel.SAFE_TIME_HEADWAY));

            this.vehicleIdx = vehicleIdx;

            // Align car with tangent of initial lane
            _angle = -initialLane.Path.TangentOfProjectedPosition(initialLane.Path.Lerp(lerpOffset)).Normal.Angle;
            _direction = new Vector2((float)Math.Cos(_angle), (float)Math.Sin(_angle));

            // Vehicle starts in a keep lane state
            DrivingState = new KeepLaneState(this, initialLane);
            DrivingState.OnEnter();
            this.vehicleParams = vehicleParams;
        }

        /// <summary>
        /// Compute update using the Intelligent Driver Model for car-following model and MOBIL lane change criterion
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Update using FSM and adjust the state accordingly for next update
            DrivingState drivingState = DrivingState.Update(deltaTime);
            if (drivingState != null)
            {
                DrivingState.OnExit();
                DrivingState = drivingState;
                DrivingState.OnEnter();
            }
        }

        /// <summary>
        /// Geometrical representation of the vehicle
        /// </summary>
        /// <returns>Rectangle representation of the vehicle</returns>
        public Rectangle GetGeometricalFigure()
        {
            return new Rectangle(Position, VehicleWidth, VehicleLength, Angle);
        }

        /// <summary>
        /// Computes vector from the two points closest to one another between two vehicles
        /// </summary>
        /// <param name="v1">First vehicle</param>
        /// <param name="v2">Second vehicle</param>
        public static Vector2 ComputeBumperToBumperVector(Vehicle v1, Vehicle v2)
        {
            // Get closest corner of c2
            Vector2 p1 = v2.GetGeometricalFigure().ClosestVertex(v1.Position);
            // Get closest corner of c1
            Vector2 p2 = v1.GetGeometricalFigure().ClosestVertex(p1);

            // Closest corner c1 <-> closest corner c2
            Vector2 bumperToBumper = p2 - p1;
            return bumperToBumper;
        }

        /// <summary>
        /// Actions to perform when a car is being removed from the world
        /// </summary>
        public void Remove()
        {
            DrivingState.OnExit();
        }
    }
}
