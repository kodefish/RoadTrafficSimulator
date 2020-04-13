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
            // Control speed -> gas pedal goes vroomvroom
            Vector2 gasPedalForce = car.Direction * Vector2.Dot(GetForce(), car.Direction);
            car.ApplyForce(gasPedalForce);

            // Control steering -> steering wheel goes whoosh
            car.ApplyTorque(GetTorque(deltaTime));

            return this;
        }

        /// <summary>
        /// Computes the acceleration / braking of the car
        /// </summary>
        /// <returns></returns>
        protected abstract Vector2 GetForce();

        /// <summary>
        /// Computes the steering. Right now uses torque to rotate 
        /// the rigid body, based on a target. Target computation
        /// based on Craig Reynolds path following behavior
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        protected virtual float GetTorque(float deltaTime)
        {
            // 1. Compute future position
            Vector2 futurePos = car.Position + car.LinearVelocity * deltaTime;

            // 2. Project future position onto the path
            Vector2 projectedFuturePosition = Path.NormalPoint(futurePos);

            // 3. Go along the path from the projected position by a wee bit to get target
            float epsilon = car.CarLength;
            Vector2 target = projectedFuturePosition + Path.TangentOfProjectedPosition(projectedFuturePosition) * epsilon;

            // 4. Compute desired angle from current position and target
            float desiredAngle = (target - car.Position).Angle;

            // 5. Compute desired angular velocity (dAngle / dt)
            // 6. Compute desired angular acceleration (dAngularVelocity / dt)
            // 7. Compute and apply torque
            // 5-7 give the following expression
            float angularAcceleration = ((desiredAngle - car.Direction.Angle) / deltaTime - car.AngularVelocity) / deltaTime;
            return angularAcceleration * car.MoI;
        }
    }
}
