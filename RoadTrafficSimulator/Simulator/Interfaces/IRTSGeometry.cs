using RoadTrafficSimulator.Simulator.DataStructures.Geometry;

namespace RoadTrafficSimulator.Simulator.Interfaces
{
    /// <summary>
    /// Interface indicates that an object has some geometrical representation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IRTSGeometry<T> where T : GeometricalFigure
    {
        /// <summary>
        /// Geometrical representation of the object
        /// </summary>
        T GetGeometricalFigure();
    }
}
