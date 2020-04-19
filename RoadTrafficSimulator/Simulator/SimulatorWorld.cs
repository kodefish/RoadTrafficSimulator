using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator
{
    class SimulatorWorld
    {
        public List<FourWayIntersection> Intersections { get; private set; }
        public List<Road> Roads { get; private set; }
        public List<Car> Cars { get; private set; }

        // TODO add cars and light controllers

        public SimulatorWorld()
        {
            Intersections = new List<FourWayIntersection>();
            Roads = new List<Road>();
            Cars = new List<Car>();
        }

        public void AddIntersection(FourWayIntersection intersection) => Intersections.Add(intersection);
        public void RemoveIntersection(FourWayIntersection intersection) => Intersections.Remove(intersection);

        public void AddRoad(Road road) => Roads.Add(road);
        public void RemoveRoad(Road road) => Roads.Remove(road);

        public void AddCar(Car car) => Cars.Add(car);
        public void RemoveCar(Car car)
        {
            car.Remove();
            Cars.Remove(car);
        }

        public void Update(float deltaTime)
        {
            if (deltaTime > 0)
            {
                // Update all the light controls
                foreach (FourWayIntersection i in Intersections) i.Update(deltaTime);

                // Update all the lanes (compute new leader cars, take care of light controllers)
                foreach (Road r in Roads) r.Update(deltaTime);

                // Update car decisions
                foreach (Car c in Cars) c.Update(deltaTime);

                // Apply the decisions using physics
                foreach (Car c in Cars) c.IntegrateForces(deltaTime);
            }
        }

    }
}
