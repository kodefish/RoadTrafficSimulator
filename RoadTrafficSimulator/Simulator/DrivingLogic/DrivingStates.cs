using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.DrivingLogic
{
    struct LeaderCarInfo
    {
        public LeaderCarInfo(float distanceToNextCar, float approachingRate)
        {
            DistToNextCar = distanceToNextCar;
            ApproachingRate = approachingRate;
        }

        public float DistToNextCar { get; }
        public float ApproachingRate { get; }
    }

    abstract class DrivingState
    {
        protected Car car;
        protected Path path;

        // Gets updated in the lane update
        public LeaderCarInfo LeaderCarInfo { get; set; }

        public DrivingState(Car car, Path path)
        {
            this.car = car;
            this.path = path;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        public abstract float MaxSpeed();

        public virtual DrivingState Update(float deltaTime)
        {
            // Control speed -> gas pedal goes vroomvroom
            car.ApplyForce(GetForce());

            // Control steering -> steering wheel goes whoosh
            car.ApplyTorque(GetTorque());

            return this;
        }

        protected abstract Vector2 GetForce();
        protected virtual float GetTorque()
        {
            return 0;
        }
    }

    class KeepLaneState : DrivingState
    {
        private Lane lane;

        public KeepLaneState(Car car, Lane lane) : base(car, lane.Path)
        {
            this.lane = lane;
        }

        public override float MaxSpeed() => Math.Min(car.MaxCarSpeed, lane.MaxSpeed);

        public override void OnEnter()
        {
            lane.AddCar(car);
        }

        public override void OnExit()
        {
            lane.RemoveCar(car);
        }

        public override DrivingState Update(float deltaTime)
        {
            DrivingState state = base.Update(deltaTime);

            // TODO Determine if I need to change lanes or if I'm at the end of a lane, and trigger a state change

            return state;
        }

        protected override Vector2 GetForce()
        {
            Vector2 idmAcceleration = IntelligentDriverModel.ComputeAccelerationIntensity(
                car, 
                path.TangentOfProjectedPosition(car.Position),
                LeaderCarInfo.DistToNextCar,
                LeaderCarInfo.ApproachingRate
                );

            // Figure out the force we have to apply on the car to reach target acceleration
            Vector2 deltaAcceleration = idmAcceleration - car.Acceleration;
            Vector2 deltaForce = deltaAcceleration * car.Mass;
            return deltaForce;
        }
    }

}
