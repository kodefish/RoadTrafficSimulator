using System;
using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Physics;
using RoadTrafficSimulator.Simulator.DrivingLogic;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    struct CarParams
    {
        public float Mass,
            CarWidth,
            CarLength,
            MaxSpeed,
            MaxAccleration,
            BrakingDeceleration;
    }

    class Car : RigidBody
    {
        // TODO: move this to driver state
        // Determines how far along a car is on it's current lane
        public Lane Lane { get; private set; }

        // Gets updated in the lane update
        public LeaderCarInfo LeaderCarInfo { get; private set; }

        public void SetLeaderCarInfo(float distToNextCar, float approachingRate)
        {
            LeaderCarInfo leaderCarInfo;
            leaderCarInfo.distToNextCar = distToNextCar;
            leaderCarInfo.approachingRate = approachingRate;

            LeaderCarInfo = leaderCarInfo;
        }

        // Car params
        private readonly CarParams carParams;

        public float MaxSpeed => Math.Min(carParams.MaxSpeed, Lane.MaxSpeed);

        // Acceleration constant params
        public float MaxAcceleration => carParams.MaxAccleration;
        public float BrakingDeceleration => carParams.BrakingDeceleration;
        public float CarWidth => carParams.CarWidth;
        public float CarLength => carParams.CarLength;

        /// <summary>
        /// Creates a car with a mass and an initial position
        /// </summary>
        /// <param name="carParams">Description of the car</param>
        /// <param name="initialLane">Starting lane</param>
        /// <param name="timeOffset">Offset along the lane trajectory</param>
        public Car(CarParams carParams, Lane initialLane, float timeOffset = 0)
            : base(carParams.Mass, ComputeCarMomentOfInertia(carParams), initialLane.Path.PathStart, (-(initialLane.Path.TangentOfProjectedPosition(initialLane.Path.PathStart).Normal)).Angle)
        {
            Lane = initialLane;
            Lane.AddCar(this);
            this.carParams = carParams;
            Vector2 laneDirection = initialLane.Path.TangentOfProjectedPosition(initialLane.Path.PathStart);
            Debug.WriteLine("Lane info - direction: {0}, normal: {1}, angle: {2}", laneDirection, -(laneDirection.Normal), (-(laneDirection.Normal)).Angle);
        }

        private static float ComputeCarMomentOfInertia(CarParams carParams)
        {
            return carParams.Mass * (carParams.CarWidth * carParams.CarWidth + carParams.CarLength * carParams.CarLength);
        }

        /// <summary>
        /// Compute update using the Intelligent Driver Model for car-following model and MOBIL lane change criterion
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            Vector2 idmAcceleration = IntelligentDriverModel.ComputeAccelerationIntensity(this, Lane.Path.TangentOfProjectedPosition(Position));

            // Figure out the force we have to apply on the car to reach target acceleration
            Vector2 deltaAcceleration = idmAcceleration - Acceleration;
            Vector2 deltaForce = deltaAcceleration * Mass;
            ApplyForce(deltaForce);
        }

        internal float ComputeDistanceToLeaderCar(Car other)
        {
            return Vector2.Distance(other.Position, Position);
            // Check that other car is in front
            Vector2 v = other.Position - Position;
            // if (Vector2.Dot(v, Direction) <= 0) throw new ArgumentException("Other car must be in front of current car!");

            // Compute rear corners of the other car
            Vector2 rearBumper = other.Position - other.Direction * other.CarLength;
            Vector2 rearLeftCorner = rearBumper + other.Direction.Normal * other.CarWidth / 2;
            Vector2 rearRightCorner = rearBumper - other.Direction.Normal * other.CarWidth / 2;

            // Check which is closer to current car
            Vector2 closestCorner = Vector2.Distance(Position, rearLeftCorner) < Vector2.Distance(Position, rearRightCorner) ? rearLeftCorner : rearRightCorner;

            // Take vector position -> closest corner and project it onto the direction of the car
            return Vector2.Dot(closestCorner - Position, Direction);
        }
    }
}
