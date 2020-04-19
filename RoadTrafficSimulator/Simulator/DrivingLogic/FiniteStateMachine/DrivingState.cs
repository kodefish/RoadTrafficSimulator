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
        protected Vehicle car;

        /// <summary>
        /// Path the driver tries to follow
        /// </summary>
        public Path Path { get; private set; }

        /// <summary>
        /// Information about car directly in front
        /// </summary>
        public Dictionary<int, LeaderCarInfo> LeaderCarInfo { get; }

        /// <summary>
        /// Reference to a simple PID controller implementation
        /// </summary>
        private PIDController pidController;

        /// <summary>
        /// Next lane to take when end of current one is reached. May be null.
        /// </summary>
        public abstract Lane NextLane { get; }

        /// <summary>
        /// Creates a car controller that follows some path
        /// </summary>
        /// <param name="car">Car to control</param>
        /// <param name="path">Path to follow</param>
        public DrivingState(Vehicle car, Path path)
        {
            this.car = car;
            this.Path = path;

            LeaderCarInfo = new Dictionary<int, LeaderCarInfo>();

            pidController = new PIDController(9.0f, 0.0f, 9.0f);
        }

        /// <summary>
        /// Allow lanes to give information about the car in front 
        /// </summary>
        /// <param name="laneIdx">Lane index of lane corresponding to info</param>
        /// <param name="leaderCarInfo">Information about car in front in lane</param>
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
        /// 1. Apply tangential force (accelerate / brake)
        /// 2. Apply normal force (steering)
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        /// <returns>Potential state change</returns>
        public virtual DrivingState Update(float deltaTime)
        {
            // Apply forward to drive the car
            Vector2 acceleration = ComputeTangentialAcceleration();

            // Only apply steering if the car is moving (can't really steer a stationnary car)
            if (car.LinearVelocity.Norm > 0) acceleration += ComputeNormalAcceleration(deltaTime);

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
        /// Computes the steering force, based on a PID controller. The controller
        /// strives to minimize the distance to the path midline
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        protected virtual Vector2 ComputeNormalAcceleration(float deltaTime)
        {
            // Get orthogonal distance to path
            Segment closestSegment = Path.Segments[Path.ClosestSegment(car.Position)];
            Vector2 target = closestSegment.ProjectOntoSupportingLine(car.Position);
            Vector2 dist = target - car.Position;
            Vector2 normal = closestSegment.Direction.Normal;
            float error = Vector2.Dot(dist, normal);
            pidController.UpdateError(error, deltaTime);
            float adjustement = pidController.PIDError();
            
            float maxSteeringAcc = (float)Math.Tan(car.MaxSteeringAngle) * car.LinearVelocity.Norm;
            if (adjustement < -maxSteeringAcc) adjustement = -maxSteeringAcc;
            if (adjustement > maxSteeringAcc) adjustement = maxSteeringAcc;

            return normal * adjustement;
        }

        /// <summary>
        /// Implementation of a map function of a value from one range to another.
        /// </summary>
        /// <param name="s">value to map</param>
        /// <param name="a1">min first range</param>
        /// <param name="a2">max first range</param>
        /// <param name="b1">min second range</param>
        /// <param name="b2">max second range</param>
        /// <returns>s mapped to second range</returns>
        private float Map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s-a1)*(b2-b1)/(a2-a1);
        }
 
    }
}
