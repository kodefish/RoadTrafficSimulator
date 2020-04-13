using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine
{
    /// <summary>
    /// Information about the car directly in front
    /// </summary>
    struct LeaderCarInfo
    {
        /// <summary>
        /// Encapsulates distance to next car and approaching rate
        /// </summary>
        /// <param name="distanceToNextCar">Bumper to bumper distance to next car</param>
        /// <param name="approachingRate">Difference in car velocities</param>
        public LeaderCarInfo(float distanceToNextCar, float approachingRate)
        {
            DistToNextCar = distanceToNextCar;
            ApproachingRate = approachingRate;
        }

        /// <summary>
        /// Distance in meters to car in front
        /// </summary>
        public float DistToNextCar { get; }
        /// <summary>
        /// Difference in velocities between the current car and car in front
        /// </summary>
        public float ApproachingRate { get; }
    }

    /// <summary>
    /// Autonomous driver finite state machine. All classes inherit the driver behavior of path following.
    /// </summary>
    abstract class DrivingState
    {
        protected Car car;

        /// <summary>
        /// Path the driver tries to follow
        /// </summary>
        public Path Path { get; private set; }

        /// <summary>
        /// Information about car directly in front
        /// </summary>
        public Dictionary<int, LeaderCarInfo> LeaderCarInfo { get; }

        /// <summary>
        /// Creates a car controller that follows some path
        /// </summary>
        /// <param name="car">Car to control</param>
        /// <param name="path">Path to follow</param>
        public DrivingState(Car car, Path path)
        {
            this.car = car;
            this.Path = path;

            LeaderCarInfo = new Dictionary<int, LeaderCarInfo>();
        }

        public void SetLeaderCarInfo(int laneIdx, LeaderCarInfo leaderCarInfo)
        {
            LeaderCarInfo[laneIdx] = leaderCarInfo;
        }

        /// <summary>
        /// Method executes on first entering the state. Can be used for 
        /// any kind of setup or initial action
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// Method executes when state is left. Can be used for cleaning up
        /// or finalising things 
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// Get the maximum driving speed based on the current state
        /// </summary>
        /// <returns>Maximum speed of the car in the current state</returns>
        public abstract float MaxSpeed();

        /// <summary>
        /// Update the driving mechanics.
        /// 1. Apply force (accelerate / brake)
        /// 2. Apply torque (steering)
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        /// <returns>Potential state change</returns>
        public virtual DrivingState Update(float deltaTime)
        {
            // Direction of the force
            Vector2 acceleration = ComputeTangentialAcceleration() + ComputeNormalAcceleration(deltaTime);

            Vector2 force = acceleration * car.Mass;            
            car.ApplyForce(force);

            return null;
        }

        /// <summary>
        /// Computes the acceleration / braking of the car
        /// </summary>
        /// <returns></returns>
        protected abstract Vector2 ComputeTangentialAcceleration();

        /// <summary>
        /// Computes the steering. Right now uses torque to rotate 
        /// the rigid body, based on a target. Target computation
        /// based on Craig Reynolds path following behavior
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        protected virtual Vector2 ComputeNormalAcceleration(float deltaTime)
        {
            // Get orthogonal distance to path
            Segment closestSegment = Path.Segments[Path.ClosestSegment(car.Position)];
            Vector2 target = closestSegment.ProjectOntoSupportingLine(car.Position);

            Vector2 desired = target - car.Position;
            float distance = desired.Norm;
            if (distance > 0)
            {
                desired = desired.Normalized * Math.Min(car.MaxCarSpeed, distance / deltaTime);
               
                // Compute normal dV 
                Vector2 dV = desired - Vector2.Dot(car.LinearVelocity, desired) * desired.Normalized;
                return desired / deltaTime;
            }
            else return new Vector2();
        }

        private float Map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s-a1)*(b2-b1)/(a2-a1);
        }
 
    }
}
