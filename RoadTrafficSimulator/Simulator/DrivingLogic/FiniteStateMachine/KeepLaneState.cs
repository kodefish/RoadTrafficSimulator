using System;
using System.Diagnostics;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine
{
    /// <summary>
    /// State that given a lane, will stick to it while respecting traffic laws
    /// and vehicles ahead
    /// </summary>
    class KeepLaneState : DrivingState
    {
        /// <summary>
        /// Lane to keep
        /// </summary>
        private Lane lane;

        /// <summary>
        /// Create lane keep state.
        /// </summary>
        /// <param name="car">Car to control</param>
        /// <param name="lane">Lane to keep</param>
        public KeepLaneState(Vehicle car, Lane lane) : base(car, lane.Path)
        {
            this.lane = lane;
        }

        /// <summary>
        /// Get the next lane. In this case we just take the next lane of the currently kept lane.
        /// </summary>
        public override Lane NextLane => lane.NextLane;

        /// <summary>
        /// Returns the min between the car's maximum speed and the lane's speed limit.
        /// </summary>
        /// <returns>Maximum speed of the car in the current state</returns>
        public override float MaxSpeed() => Math.Min(car.MaxVehicleSpeed, lane.MaxSpeed);

        /// <summary>
        /// Add the car to the lane.
        /// </summary>
        public override void OnEnter()
        {
            lane.AddVehicle(car);
        }

        /// <summary>
        /// Remove the car from the lane.
        /// </summary>
        public override void OnExit()
        {
            lane.RemoveVehicle(car);
        }

        /// <summary>
        /// Determine a state change based on if car should to change lanes
        /// or car has reached the end of it's lane.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        /// <returns></returns>
        public override DrivingState Update(float deltaTime)
        {
            DrivingState state = base.Update(deltaTime);
            // Determine if I need to change lanes or if I'm at the end of a lane, and trigger a state change
            float lerp = Path.InverseLerp(car.Position + car.Direction * car.VehicleLength / 2);
            if (lerp >= 0.99 && NextLane != null) // End of the lane reached
            {
                state = new KeepLaneState(car, NextLane); // New wait for light state if next lane is available
            }
            else if (0.2 < lerp && lerp < 0.8) // Allow lane changing from the first 20% of the lane, until 80%
            {
                // Check via MOBIL for potential lane change
                Lane newLane = Mobil.OptimalLane(car, lane);
                if (newLane.LaneIdx != lane.LaneIdx) 
                    state = new ChangeLaneState(car, lane, newLane);
            }
            return state;
        }

        /// <summary>
        /// Apply Intelligent Driver Model based acceleration to the vehicle
        /// </summary>
        /// <returns>Acceleration given by IDM</returns>
        protected override Vector2 ComputeTangentialAcceleration()
        {
            Vector2 laneDir = lane.Path.TangentOfProjectedPosition(car.Position);
            Vector2 laneDirAcc = IntelligentDriverModel.ComputeAccelerationIntensity(
                car, 
                laneDir,
                LeaderVehicleInfo[lane.LaneIdx].DistToNextCar,
                LeaderVehicleInfo[lane.LaneIdx].ApproachingRate
                );
            
            Vector2 vehicleDir = Path.TangentOfProjectedPosition(car.Position);
            Vector2 longitudinalAcc = Vector2.Dot(laneDirAcc, vehicleDir) * vehicleDir;
            return longitudinalAcc;
        }
    }

}
