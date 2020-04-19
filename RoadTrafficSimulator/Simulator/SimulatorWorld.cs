using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator
{
    class SimulatorWorld
    {
        /// <summary>
        /// List of intersections
        /// </summary>
        public List<FourWayIntersection> Intersections { get; private set; }

        /// <summary>
        /// List of roads
        /// </summary>
        public List<Road> Roads { get; private set; }

        /// <summary>
        /// List of cars
        /// </summary>
        public List<Vehicle> Cars { get; private set; }

        /// <summary>
        /// Simulator world that takes care of updating intersections, roads, and cars
        /// </summary>
        public SimulatorWorld()
        {
            Intersections = new List<FourWayIntersection>();
            Roads = new List<Road>();
            Cars = new List<Vehicle>();
        }

        /// <summary>
        /// Add intersection to the world
        /// </summary>
        public void AddIntersection(FourWayIntersection intersection) => Intersections.Add(intersection);

        /// <summary>
        /// Remove intersection from the world
        /// </summary>
        public void RemoveIntersection(FourWayIntersection intersection)
        {
            // Remove any cars that may be on the intersection
            foreach(List<Lane> ll in intersection.CurrentTrafficLightState.activeLanes.Values)
            {
                foreach (Lane l in ll)
                {
                    while (l.Vehicles.Count > 0) RemoveCar(l.Vehicles[0]);
                }
            }

            // Remove any roads connected to intersection
            while (intersection.Roads.Count > 0) RemoveRoad(intersection.Roads[0]);
            Intersections.Remove(intersection);
        }

        /// <summary>
        /// Add road to the world
        /// </summary>
        public void AddRoad(Road road) => Roads.Add(road);

        /// <summary>
        /// Remove road from the world
        /// </summary>
        public void RemoveRoad(Road road)
        {
            // Remove any vehicles that may be on the road
            foreach (Lane l in road.InLanes)
                while (l.Vehicles.Count > 0) RemoveCar(l.Vehicles[0]);

            foreach (Lane l in road.OutLanes)
                while (l.Vehicles.Count > 0) RemoveCar(l.Vehicles[0]);

            road.Remove();
            Roads.Remove(road);
        }

        /// <summary>
        /// Add car to the world
        /// </summary>
        public void AddCar(Vehicle car) => Cars.Add(car);

        /// <summary>
        /// Remove car from the world
        /// </summary>
        public void RemoveCar(Vehicle car)
        {
            car.Remove();

            // Perform any car cleanup you need
            Cars.Remove(car);
        }

        /// <summary>
        /// Update simulation
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            if (deltaTime > 0)
            {
                // Update all the light controls
                foreach (FourWayIntersection i in Intersections) i.Update(deltaTime);

                // Update all the lanes (compute new leader cars, take care of light controllers)
                foreach (Road r in Roads) r.Update(deltaTime);

                // Update car decisions
                foreach (Vehicle c in Cars) c.Update(deltaTime);

                // Apply the decisions using physics
                foreach (Vehicle c in Cars) c.IntegrateForces(deltaTime);
            }
        }

    }
}
