using System;
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
        public float TrajectoryStep { get; private set; } 
        public Vector2 TrajectoryDirection { get; private set; }
        private Lane lane;

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

        public float MaxSpeed => Math.Min(carParams.MaxSpeed, lane.MaxSpeed);

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
            lane = initialLane;
            lane.AddCar(this);
            this.carParams = carParams;

            // TODO set this to end of lane
            // SetLeaderCarInfo()

            UpdateTrajectoryInformation();
        }

        /// <summary>
        /// Compute update using the Intelligent Driver Model for car-following model and MOBIL lane change criterion
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Reset car to start of lane
            if (lane.ReachedEndOfLane(Position))
            {
                Position = lane.Trajectory.GetPosition(1);
            }
            else
            {
                UpdateTrajectoryInformation();

                // Get car directly in front of this one in the lane
                // Debug.WriteLine("Car pos: {0}, dist to next car: {1}", Position, LeaderCarInfo.distToNextCar);
                float idmAccIntensity = IntelligentDriverModel.ComputeAccelerationIntensity(this);

                // Update forces acting on the car
                float step = TrajectoryStep;
                Vector2 targetAcceleration = TrajectoryDirection * idmAccIntensity;
                // Debug.WriteLine("Target acc: {0}", targetAcceleration);
                Vector2 deltaAcceleration = targetAcceleration - Acceleration;
                Vector2 deltaForce = deltaAcceleration * Mass;
                ApplyForce(deltaForce);
            }
        }

        private void UpdateTrajectoryInformation()
        {
            // Update trajectory information
            TrajectoryStep = lane.GetProgression(Position);
            TrajectoryDirection = lane.Trajectory.GetTangent(TrajectoryStep).Normalized;
        }
    }
}
