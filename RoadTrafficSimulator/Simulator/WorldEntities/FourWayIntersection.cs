using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.Interfaces;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    /// <summary>
    /// Supports up-to four incoming roads 
    /// </summary>
    class FourWayIntersection : IRTSUpdateable, IRTSGeometry<Rectangle>
    {
        // Intersection information
        public Vector2 Origin { get; private set; }

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

        public FourWayIntersection(Vector2 origin)
        {
            Origin = origin;
            roads = new List<Road>();
        }

        // Adds road to intersection, if possible
        public void AddRoad(Road road)
        {
            if (roadCount < MAX_ROADS)
            {
                roads.Add(road);
                ComputeRoadGeometry();
            }
            else throw new ArgumentOutOfRangeException(String.Format("Max number ({0}) roads already reached!", roadCount));
        }

        public void RemoveRoad(Road road)
        {
            bool ok = roads.Remove(road);
            if (ok) ComputeRoadGeometry();
        }

        public void ComputeRoadGeometry()
        {
            foreach (Road r in roads) r.ComputeLaneGeometry();
        }

        public Vector2 Dimensions => new Vector2(Width, Height);

        public Segment GetRoadSegment(FourWayIntersection other, float roadWidth)
        {
            Vector2 direction = (other.Origin - this.Origin).Normalized;
            Vector2 source = new Vector2(), target = new Vector2();
            if (direction.Equals(Vector2.Up))
            {
                source = new Vector2(Origin.X - roadWidth / 2, Origin.Y + Height / 2);
                target = new Vector2(Origin.X + roadWidth / 2, Origin.Y + Height / 2);
            }
            else if (direction.Equals(Vector2.Down))
            {
                source = new Vector2(Origin.X + roadWidth / 2, Origin.Y - Height / 2);
                target = new Vector2(Origin.X - roadWidth / 2, Origin.Y - Height / 2);
            }
            else if (direction.Equals(Vector2.Left))
            {
                source = new Vector2(Origin.X - Width / 2, Origin.Y - roadWidth / 2);
                target = new Vector2(Origin.X - Width / 2, Origin.Y + roadWidth / 2);
            }
            else if (direction.Equals(Vector2.Right))
            {
                source = new Vector2(Origin.X + Width / 2, Origin.Y + roadWidth / 2);
                target = new Vector2(Origin.X + Width / 2, Origin.Y - roadWidth / 2);

            }

            return new Segment(source, target);
        }

        public void Update(float deltaTime)
        {
            // TODO update light controllers
            throw new NotImplementedException();
        }

        public Rectangle GetGeometricalFigure()
        {
            return new Rectangle(Origin, Width, Height);
        }
    }
}
