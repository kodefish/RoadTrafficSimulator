﻿using System;
using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Physics;
using RoadTrafficSimulator.Simulator.DrivingLogic;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    struct CarParams
    {
        public float Mass, MaxSpeed, MaxAccleration, BrakingDeceleration, CarLength;
    }

    class Car : RigidBody
    {
        // TODO: move this to driver state
        // Determines how far along a car is on it's current lane
        public float TrajectoryStep => Lane.GetProgression(Position);
        public Vector2 TrajectoryDirection => Lane.Trajectory.GetTangent(TrajectoryStep).Normalized;
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
        public float CarLength => carParams.CarLength;

        /// <summary>
        /// Creates a car with a mass and an initial position
        /// </summary>
        /// <param name="carParams">Description of the car</param>
        /// <param name="initialLane">Starting lane</param>
        /// <param name="timeOffset">Offset along the lane trajectory</param>
        public Car(CarParams carParams, Lane initialLane, float timeOffset = 0) 
            : base(carParams.Mass, initialLane.Trajectory.GetPosition(timeOffset))
        {
            Lane = initialLane;
            Lane.AddCar(this);
            this.carParams = carParams;
        }

        /// <summary>
        /// Compute update using the Intelligent Driver Model for car-following model and MOBIL lane change criterion
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Get car directly in front of this one in the lane
            float idmAccIntensity = IntelligentDriverModel.ComputeAccelerationIntensity(this, Lane.Direction);

            // Update forces acting on the car
            Vector2 targetAcceleration = Lane.Direction * idmAccIntensity;

            // Figure out the force we have to apply on the car to reach target acceleration
            Vector2 deltaAcceleration = targetAcceleration - Acceleration;
            Vector2 deltaForce = deltaAcceleration * Mass;
            ApplyForce(deltaForce);
        }
    }
}
