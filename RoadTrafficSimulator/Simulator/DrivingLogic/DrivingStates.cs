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
        public Path Path { get; private set; }

        // Gets updated in the lane update
        public LeaderCarInfo LeaderCarInfo { get; set; }

        public DrivingState(Car car, Path path)
        {
            this.car = car;
            this.Path = path;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        public abstract float MaxSpeed();

        public virtual DrivingState Update(float deltaTime)
        {
            // Control speed -> gas pedal goes vroomvroom
            Vector2 gasPedalForce = car.Direction * Vector2.Dot(GetForce(), car.Direction);
            car.ApplyForce(gasPedalForce);

            // Control steering -> steering wheel goes whoosh
            car.ApplyTorque(GetTorque(deltaTime));

            return this;
        }

        protected abstract Vector2 GetForce();

        // TODO: Craig Renolds based path following behavior
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
            float desiredAngle = (target - car.Position).Angle + (float) Math.PI / 2;

            // 5. Compute desired angular velocity (dAngle / dt)
            // 6. Compute desired angular acceleration (dAngularVelocity / dt)
            // 7. Compute and apply torque
            // 5-7 give the following expression
            float angularAcceleration = ((desiredAngle - car.Angle) / deltaTime - car.AngularVelocity) / deltaTime;
            return angularAcceleration * car.MoI;
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
                Path.TangentOfProjectedPosition(car.Position),
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
