using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.Interfaces
{
    interface IRTSDimension
    {
        /// <summary>
        /// Get the dimensions of an object, width and height (assumus everything to be rectangles)
        /// </summary>
        /// <returns></returns>
        Vector2 Dimensions { get; }
    }
}
