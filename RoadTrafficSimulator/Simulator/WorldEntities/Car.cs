using System;
using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Physics;
using RoadTrafficSimulator.Simulator.DrivingLogic;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;

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

    class Car : RigidBody, IRTSGeometry<Rectangle>
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

        public Rectangle GetGeometricalFigure()
        {
            return new Rectangle(Position, CarWidth, CarLength, Angle);
        }

        public static Vector2 ComputeBumperToBumperVector(Car c1, Car c2)
        {
            // Get closest corner of c2
            Vector2 p1 = c2.GetGeometricalFigure().ClosestVertex(c1.Position);
            // Get closest corner of c1
            Vector2 p2 = c1.GetGeometricalFigure().ClosestVertex(p1);

            // Closest corner c1 <-> closest corner c2
            Vector2 bumperToBumper = p2 - p1;
            return bumperToBumper;
        }
    }
}
