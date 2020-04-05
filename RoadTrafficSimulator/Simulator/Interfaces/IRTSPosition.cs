using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.Interfaces
{
    interface IRTSPosition
    {
        /// <summary>
        /// Get the position of the object, to place it in the RTS simulation world
        /// </summary>
        /// <returns></returns>
        Vector2 Position { get; set; }
    }
}
