using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadTrafficSimulator.DataStructures;
using RoadTrafficSimulator.Interfaces;

namespace RoadTrafficSimulator.Simulator
{
    /// <summary>
    /// Supports up-to four incoming roads 
    /// </summary>
    class FourWayIntersection : IRTSDimension, IRTSPosition
    {
        // Intersection information
        private readonly IRTSPosition _origin;  // Center of the intersection
        public Vector2 Origin => _origin.Position;

        // Road information
        private static int MAX_ROADS = 4;
        private List<Road> roads;
        private int roadCount { get => roads.Count; }

        public float Width
        {
            get
            {
                float maxHorizontalRoadWidth = 1;
                foreach (Road r in roads)
                {
                    if (!r.IsHorizontal) maxHorizontalRoadWidth = Math.Max(maxHorizontalRoadWidth, r.RoadWidth);
                }
                return maxHorizontalRoadWidth;
            }
        }

        public float Height
        {
            get
            {
                float maxVerticalRoadWidth = 1;
                foreach (Road r in roads)
                {
                    if (r.IsHorizontal) maxVerticalRoadWidth = Math.Max(maxVerticalRoadWidth, r.RoadWidth);
                }
                return maxVerticalRoadWidth;
            }
        }

        public FourWayIntersection(IRTSPosition origin)
        {
            _origin = origin;
            roads = new List<Road>();
        }

        // Adds road to intersection, if possible
        public void AddRoad(Road road)
        {
            if (roadCount < MAX_ROADS)
            {
                roads.Add(road);
            }
            else throw new ArgumentOutOfRangeException(String.Format("Max number ({0}) roads already reached!", roadCount));
        }

        public void RemoveRoad(Road road) => roads.Remove(road);

        public Vector2 GetDimensions()
        {
            return new Vector2(Width, Height);
        }

        public Vector2 Position
        {
            get { return _origin.Position; }
            set { _origin.Position = value; }
        }
    }
}
