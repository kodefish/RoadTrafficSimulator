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
        private Lane currentLane, nextLane;

        /// <summary>
        /// Create lane keep state.
        /// </summary>
        /// <param name="car">Car to control</param>
        /// <param name="currentLane">Lane car is changing from</param>
        /// <param name="nextLane">Lane car is changing to</param>
        public ChangeLaneState(Car car, Lane currentLane, Lane nextLane) 
            : base(car, Path.TransitionPath(currentLane.Path, nextLane.Path, car.Position, car.CarLength, car.Direction))
        {
            if (currentLane.LaneIdx == nextLane.LaneIdx) throw new Exception("Lane change must occur between different lanes!");
            this.currentLane = currentLane;
            this.nextLane = nextLane;
        }

        /// <summary>
        /// Returns the min between the car's maximum speed and the two lanes' speed limits
        /// </summary>
        /// <returns>Maximum speed of the car in the current state</returns>
        public override float MaxSpeed() => Math.Min(car.MaxCarSpeed, Math.Min(currentLane.MaxSpeed, nextLane.MaxSpeed));

        /// <summary>
        /// Add the car to the lane the car is merging into
        /// </summary>
        public override void OnEnter()
        {
            currentLane.AddCar(car);
            nextLane.AddCar(car);
        }

        /// <summary>
        /// Remove the car from the two lanes (let the next state handle it)
        /// </summary>
        public override void OnExit()
        {
            currentLane.RemoveCar(car);
            nextLane.RemoveCar(car);
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
            if (Path.DistanceToPath(car.Position) <= Path.Radius)
                state = new KeepLaneState(car, nextLane);
            return state;
        }

        /// <summary>
        /// Apply Intelligent Driver Model based acceleration to the vehicle for both lanes, and use the min of the two
        /// </summary>
        /// <returns>Force based on acceleration given by IDM</returns>
        protected override Vector2 GetForce()
        {
            // Get the acceleration in both lanes
            Vector2 currentIdmAcceleration = IdmAcceleration(currentLane.LaneIdx);
            Vector2 nextIdmAcceleration = IdmAcceleration(nextLane.LaneIdx);

            // Stay safe and take the min of the two -> don't crash for sure
            Vector2 idmAcceleration = currentIdmAcceleration.Norm < nextIdmAcceleration.Norm ? currentIdmAcceleration : nextIdmAcceleration;

            // Figure out the force we have to apply on the car to reach target acceleration
            Vector2 deltaAcceleration = idmAcceleration - car.Acceleration;
            Vector2 deltaForce = deltaAcceleration * car.Mass;
            return deltaForce;
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
                LeaderCarInfo[laneIdx].DistToNextCar,
                LeaderCarInfo[laneIdx].ApproachingRate
                );

        }
    }

}
