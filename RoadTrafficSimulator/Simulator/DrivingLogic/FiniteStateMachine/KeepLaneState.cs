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
        private Lane lane;


        /// <summary>
        /// Create lane keep state.
        /// </summary>
        /// <param name="car">Car to control</param>
        /// <param name="lane">Lane to keep</param>
        public KeepLaneState(Car car, Lane lane) : base(car, lane.Path)
        {
            this.lane = lane;
        }

        public override Lane NextLane => lane.NextLane;

        /// <summary>
        /// Returns the min between the car's maximum speed and the lane's speed limit
        /// </summary>
        /// <returns>Maximum speed of the car in the current state</returns>
        public override float MaxSpeed() => Math.Min(car.MaxCarSpeed, lane.MaxSpeed);

        /// <summary>
        /// Add the car to the lane
        /// </summary>
        public override void OnEnter()
        {
            lane.AddCar(car);
        }

        /// <summary>
        /// Remove the car from the lane
        /// </summary>
        public override void OnExit()
        {
            lane.RemoveCar(car);
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
            // TODO Determine if I need to change lanes or if I'm at the end of a lane, and trigger a state change
            float lerp = Path.InverseLerp(car.Position + car.Direction * car.CarLength / 2);
            if (lerp >= 0.99 && NextLane != null) // End of the lane reached
            {
                state = new KeepLaneState(car, NextLane); // New wait for light state
            }
            else if (0.2 < lerp && lerp < 0.8)
            {
                // Check via MOBIL for potential lane change
                Lane newLane = Mobil.OptimalLane(car, lane);
                if (newLane.LaneIdx != lane.LaneIdx) // Debug.WriteLine("Changine lane from {0} to {1}", lane.LaneIdx, newLane.LaneIdx);
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
                LeaderCarInfo[lane.LaneIdx].DistToNextCar,
                LeaderCarInfo[lane.LaneIdx].ApproachingRate
                );
            
            Vector2 vehicleDir = Path.TangentOfProjectedPosition(car.Position);
            Vector2 longitudinalAcc = Vector2.Dot(laneDirAcc, vehicleDir) * vehicleDir;
            return longitudinalAcc;
        }
    }

}
