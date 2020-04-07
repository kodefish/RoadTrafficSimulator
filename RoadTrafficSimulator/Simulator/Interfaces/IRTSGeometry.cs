using RoadTrafficSimulator.Simulator.DataStructures.Geometry;

namespace RoadTrafficSimulator.Simulator.Interfaces
{
    interface IRTSGeometry<T> where T : GeometricalFigure
    {
        T GetGeometricalFigure();
    }
}
