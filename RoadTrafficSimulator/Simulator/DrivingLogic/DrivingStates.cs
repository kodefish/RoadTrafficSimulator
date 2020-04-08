using System;
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
            throw new NotImplementedException();
        }
    }

}
