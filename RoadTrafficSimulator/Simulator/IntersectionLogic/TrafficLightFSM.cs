using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.WorldEntities;

namespace RoadTrafficSimulator.Simulator.IntersectionLogic
{
    class TrafficLightFSM
    {
        public Dictionary<Segment, List<Lane>> activeLanes;

        public TrafficLightFSM() {
            activeLanes = new Dictionary<Segment, List<Lane>>();
        }

        public void AddActiveLane(Segment key, Lane l) 
        {
            if (!activeLanes.ContainsKey(key)) activeLanes.Add(key, new List<Lane>());
            activeLanes[key].Add(l);
        }

        public List<Lane> GetPossibleNextLanes(Segment s)
        {
            try { return activeLanes[s]; }
            catch (KeyNotFoundException) { return new List<Lane>(); }
        }

        public void Clear() => activeLanes.Clear();
    }
}