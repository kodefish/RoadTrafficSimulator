using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.XNAHelpers
{
    class Intersection : IRTSPosition
    {
        public Vector2 Position { get; set; }

        public Intersection(Vector2 position)
        {
            this.Position = position;
        }
    }
}
