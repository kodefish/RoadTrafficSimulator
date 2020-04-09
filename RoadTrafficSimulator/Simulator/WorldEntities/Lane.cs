using System.Diagnostics;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DrivingLogic;
using System;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    class Lane : IRTSUpdateable
    {
        // Lane params
        public const float LANE_WIDTH = 2;          // Width of a lane, in meters
        public float MaxSpeed { get; private set; } // Speedlimit

        // Lane geometry
        private Segment _sourceSegment;
        public Segment _targetSegment;
        public Segment SourceSegment {
            get => _sourceSegment;
            set {
                _sourceSegment = value;
                UpdateMidlineAndTrajectory();
            }
        }
        public Segment TargetSegment {
            get => _targetSegment;
            set {
                _targetSegment = value;
                UpdateMidlineAndTrajectory();
            }
        }

        // Lane logic
        private List<Car> cars;
        public Path Path { get; private set; }

        /// <summary>
        /// Contruct a lane between two segments
        /// </summary>
        public Lane(float speedLimit)
        {
            MaxSpeed = speedLimit;

            // Keep track of cars on the lane
            cars = new List<Car>();
        }

        private void UpdateMidlineAndTrajectory()
        {
            // Trajectoy is just a straight line between the source segment middle to the target segment middle
            if (SourceSegment != null && TargetSegment != null)
            {
                Segment midline = new Segment(SourceSegment.Midpoint, TargetSegment.Midpoint);
                BezierCurve midlineCurve = new BezierCurve(
                    midline.Source, midline.Source + midline.Direction,
                    midline.Target, midline.Target + midline.Direction);
                Path = Path.FromBezierCurve(midlineCurve, 1); // Only use one sample, since we know it's a straight line
            }
        }

        public void AddCar(Car c) => cars.Add(c);
        public void RemoveCar(Car c) => cars.Remove(c);

        public void Update(float deltaTime)
        {
            if (cars.Count == 0) return;
            // 1. Sort cars by timestep
            cars.Sort((a, b) => Path.DistanceOfProjectionAlongPath(a.Position).CompareTo(Path.DistanceOfProjectionAlongPath(b.Position)));

            // For all the cars that have a car in front of them
            float distToNextCar, approachingRate;
            for (int i = 0; i < cars.Count - 1; i++)
            {
                distToNextCar = cars[i].ComputeDistanceToLeaderCar(cars[i + 1]);
                approachingRate = Vector2.Distance(cars[i + 1].LinearVelocity, cars[i].LinearVelocity);
                cars[i].SetLeaderCarInfo(distToNextCar, approachingRate);
            }

            // Car that is at the head of the lane
            // TODO do some fancy shit with the next lane based on intersection lights
            Car leader = cars[cars.Count - 1];

            // Just set the end of the lane
            distToNextCar = Vector2.Distance(
                leader.Position,
                Path.PathEnd + Path.TangentOfProjectedPosition(Path.PathEnd) * IntelligentDriverModel.MIN_BUMPER_TO_BUMPER_DISTANCE); 
            approachingRate = leader.LinearVelocity.Length;
            leader.SetLeaderCarInfo(distToNextCar, approachingRate);
        }

        public float DistanceToFirstCar()
        {
            if (cars.Count > 0) return Path.DistanceOfProjectionAlongPath(cars[0].Position);
            else return float.PositiveInfinity;
        }
    }
}
