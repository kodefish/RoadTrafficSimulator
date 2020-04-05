using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.DrivingLogic
{
    abstract class DrivingState
    {
        // public abstract void EnterState();
        public abstract Vector2 GetTargetAccleration();
    }

    class KeepLaneState : DrivingState
    {
        private Lane lane;
        private Car car;

        public KeepLaneState(Lane lane, Car car)
        {
            this.lane = lane;
        }

        public override Vector2 GetTargetAccleration()
        {
            float idmAcc = 5; // Add IDM static class with actual calculations based on vehicle positon + lane state, etc
            float step = lane.GetProgression(car.Positon);
            return lane.Trajectory.GetTangent(step).Normalized * idmAcc;
        }
    }

}
