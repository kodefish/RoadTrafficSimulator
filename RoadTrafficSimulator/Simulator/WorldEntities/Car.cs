﻿using System;
using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Physics;
using RoadTrafficSimulator.Simulator.DrivingLogic;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    struct CarParams
    {
        public CarParams(float mass,
            float carWidth,
            float carLength,
            float maxSpeed,
            float maxAccleration,
            float brakingDeceleration,
            float politenessFactor)
        {
            Mass = mass;
            CarWidth = carWidth;
            CarLength = carLength;
            MaxSpeed = maxSpeed;
            MaxAccleration = maxAccleration;
            BrakingDeceleration = brakingDeceleration;
            PolitenessFactor = politenessFactor;
        }
        public float Mass { get; }
        public float CarWidth { get; }
        public float CarLength { get; }
        public float MaxSpeed { get; }
        public float MaxAccleration { get; }
        public float BrakingDeceleration { get; }
        public float PolitenessFactor { get; }

    }

    class Car : RigidBody, IRTSGeometry<Rectangle>
    {
        // Car params
        private readonly CarParams carParams;

        // Acceleration constant params
        public float MaxCarSpeed => carParams.MaxSpeed;
        public float MaxAcceleration => carParams.MaxAccleration;
        public float BrakingDeceleration => carParams.BrakingDeceleration;
        public float CarWidth => carParams.CarWidth;
        public float CarLength => carParams.CarLength;
        public float PolitnessFactor => carParams.PolitenessFactor;

        // AI Finite state machine
        public DrivingState DrivingState { get; private set; }
        public float MaxOverrallSpeed => DrivingState.MaxSpeed();


        /// <summary>
        /// Creates a car with a mass and an initial position
        /// </summary>
        /// <param name="carParams">Description of the car</param>
        /// <param name="initialLane">Starting lane</param>
        /// <param name="lerpOffset">Offset along the lane trajectory</param>
        public Car(CarParams carParams, Lane initialLane, float lerpOffset = 0)
            : base(
                carParams.Mass, 
                ComputeCarMomentOfInertia(carParams), 
                initialLane.Path.Lerp(lerpOffset), 
                (-(initialLane.Path.TangentOfProjectedPosition(initialLane.Path.Lerp(lerpOffset)).Normal)).Angle)
        {
            // Make sure vehicle respects min and max acceleration params
            if (carParams.MaxAccleration < IntelligentDriverModel.MIN_ACCELERATION) 
                throw new ArgumentException(String.Format("Car acceleration ({0} m/s2) too low! Min: {1} m/s2", carParams.MaxAccleration, IntelligentDriverModel.MIN_ACCELERATION));
            if (carParams.BrakingDeceleration > IntelligentDriverModel.MAX_BRAKING) 
                throw new ArgumentException(String.Format("Car braking ({0} m/s2) too high! Max: {1} m/s2", carParams.MaxAccleration, IntelligentDriverModel.MAX_BRAKING));

            DrivingState = new KeepLaneState(this, initialLane);
            DrivingState.OnEnter();
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
            // Update using FSM and adjust the state accordingly for next update
            DrivingState drivingState = DrivingState.Update(deltaTime);
            if (drivingState != null)
            {
                DrivingState.OnExit();
                DrivingState = drivingState;
                DrivingState.OnEnter();
            }
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
