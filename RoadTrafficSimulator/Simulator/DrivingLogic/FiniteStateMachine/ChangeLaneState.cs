using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine
{
    /// <summary>
    /// State that given a lane, will stick to it while respecting traffic laws
    /// and vehicles ahead
    /// </summary>
    class ChangeLaneState : DrivingState
    {
        /// <summary>
        /// Lanes that the vehicle is in
        /// </summary>
        private Lane currentLane, nextLane;

        /// <summary>
        /// Create lane change state. The  path is just set to the next lane's path,
        /// and the PID controller takes care of providing a smooth lane change. During
        /// the change lane state, the vehicle is considered to be in both lanes at
        /// once, therefore vehicles in both lanes will consider the lane changing car 
        /// to be their leader.
        /// </summary>
        /// <param name="car">Car to control</param>
        /// <param name="currentLane">Lane car is changing from</param>
        /// <param name="nextLane">Lane car is changing to</param>
        public ChangeLaneState(Vehicle car, Lane currentLane, Lane nextLane) 
            : base(car, nextLane.Path)
        {
            if (currentLane.LaneIdx == nextLane.LaneIdx) throw new Exception("Lane change must occur between different lanes!");
            this.currentLane = currentLane;
            this.nextLane = nextLane;
        }

        /// <summary>
        /// Next lane corresponds to target lane's next lane
        /// </summary>
        public override Lane NextLane => nextLane.NextLane;

        /// <summary>
        /// Returns the min between the car's maximum speed and the two lanes' speed limits
        /// </summary>
        /// <returns>Maximum speed of the car in the current state</returns>
        public override float MaxSpeed() => Math.Min(car.MaxVehicleSpeed, Math.Min(currentLane.MaxSpeed, nextLane.MaxSpeed));

        /// <summary>
        /// Add the car to the lane the car is merging into
        /// </summary>
        public override void OnEnter()
        {
            currentLane.AddVehicle(car);
            nextLane.AddVehicle(car);
        }

        /// <summary>
        /// Remove the car from the two lanes (let the next state handle it)
        /// </summary>
        public override void OnExit()
        {
            currentLane.RemoveVehicle(car);
            nextLane.RemoveVehicle(car);
        }

        /// <summary>
        /// Determine a state change based on if car should to change lanes
        /// or car has reached the end of it's lane
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public override DrivingState Update(float deltaTime)
        {
            DrivingState state = base.Update(deltaTime);
            // Determine if lane change is complete (car is close enough to next lane trajectory)
            // Sterring behavior will kick in and complete the change
            float distanceToNextPath = nextLane.Path.DistanceToPath(car.Position);
            if (distanceToNextPath <= Path.Radius)
                state = new KeepLaneState(car, nextLane);
            return state;
        }

        /// <summary>
        /// Apply Intelligent Driver Model based acceleration to the vehicle for both lanes, and use the min of the two
        /// </summary>
        /// <returns>Force based on acceleration given by IDM</returns>
        protected override Vector2 ComputeTangentialAcceleration()
        {
            // Get the acceleration in both lanes
            Vector2 currentIdmAcceleration = IdmAcceleration(currentLane.LaneIdx);
            Vector2 nextIdmAcceleration = IdmAcceleration(nextLane.LaneIdx);

            // Stay safe and take the min of the two -> don't crash for sure
            return currentIdmAcceleration.Norm < nextIdmAcceleration.Norm ? currentIdmAcceleration : nextIdmAcceleration;
        }

        /// <summary>
        /// Get the Intelligent Driver Model from a lane, just a helper method
        /// </summary>
        /// <returns>Force based on acceleration given by IDM</returns>
        private Vector2 IdmAcceleration(int laneIdx)
        {
            return IntelligentDriverModel.ComputeAccelerationIntensity(
                car, 
                Path.TangentOfProjectedPosition(car.Position),
                LeaderVehicleInfo[laneIdx].DistToNextCar,
                LeaderVehicleInfo[laneIdx].ApproachingRate
                );
        }
    }

}
