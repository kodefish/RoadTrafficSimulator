using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator
{
    class SimulatorWorld
    {
        public List<FourWayIntersection> Intersections { get; private set; }
        public List<Road> Roads { get; private set; }

        // TODO add cars and light controllers

        public SimulatorWorld()
        {
            Intersections = new List<FourWayIntersection>();
            Roads = new List<Road>();
        }

        public void AddIntersection(FourWayIntersection intersection) => Intersections.Add(intersection);
        public void RemoveIntersection(FourWayIntersection intersection) => Intersections.Remove(intersection);

        public void AddRoad(Road road) => Roads.Add(road);
        public void RemoveRoad(Road road) => Roads.Remove(road);

        public void Update(float deltaTime)
        {
            // TODO Update cars and light controllers
            throw new NotImplementedException();
        }

    }
}
