using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.DrivingLogic
{
    abstract class DrivingState
    {
        public abstract void EnterState();
        public abstract Vector2 GetTargetAccleration();
    }

    class KeepLaneState : DrivingState
    {
        public override void EnterState()
        {
            throw new System.NotImplementedException();
        }

        public override Vector2 GetTargetAccleration()
        {
            throw new System.NotImplementedException();
        }
    }

}
