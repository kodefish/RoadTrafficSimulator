using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DrivingLogic;

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

        // Think of it like a rail
        public Segment Midline { get; private set; }
        public BezierCurve Trajectory { get; private set; }
        public Vector2 Direction => Midline.Direction;

        // Lane logic
        private List<Car> cars;

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
                Midline = new Segment(SourceSegment.Midpoint, TargetSegment.Midpoint);
                Trajectory = new BezierCurve(Midline.Source, Midline.Source + Midline.Direction / 100, Midline.Target, Midline.Target + Midline.Direction / 100);
            }
        }

        public void AddCar(Car c) => cars.Add(c);
        public void RemoveCar(Car c) => cars.Remove(c);

        private float Clamp(float x, float min, float max)
        {
            return  (x < min) ? min : ((x > max) ? max : x);
        }

        /// <summary>
        /// Returns how far along the lane the position is, 0 being at the source, 1 being at the target
        /// </summary>
        /// <param name="positon">Position in global coordinates</param>
        /// <returns></returns>
        public float GetProgression(Vector2 positon)
        {
            // check if position is on the line specified by the two points
            // if (!Midline.PointOnSegment(positon)) throw new ArgumentException(String.Format("{0} is not in the lane!", positon));
            return Clamp(Vector2.Distance(Midline.Source, positon) / Midline.Length, 0, 1);
        }

        public bool ReachedEndOfLane(Vector2 position)
        {
            return GetProgression(position) >= 0.995f;
        }

        public void Update(float deltaTime)
        {
            if (cars.Count == 0) return;
            // TODO create lookup table to get leader car
            // 1. Sort cars by timestep
            cars.Sort((a, b) => a.TrajectoryStep.CompareTo(b.TrajectoryStep));

            // For all the cars that have a car in front of them
            float distToNextCar, approachingRate;
            for (int i = 0; i < cars.Count - 1; i++)
            {
                distToNextCar = Vector2.Distance(
                    cars[i].Position,
                    cars[i + 1].Position - cars[i + 1].TrajectoryDirection * cars[i + 1].CarLength);
                approachingRate = Vector2.Distance(cars[i + 1].LinearVelocity, cars[i].LinearVelocity);
                cars[i].SetLeaderCarInfo(distToNextCar, approachingRate);
            }

            // Car that is at the head of the lane
            // TODO do some fancy shit with the next lane based on intersection lights
            Car leader = cars[cars.Count - 1];

            // Just set the end of the lane
            distToNextCar = Vector2.Distance(
                leader.Position,
                Midline.Target + Midline.Direction * IntelligentDriverModel.MIN_BUMPER_TO_BUMPER_DISTANCE); 
            approachingRate = leader.LinearVelocity.Length;
            leader.SetLeaderCarInfo(distToNextCar, approachingRate);
        }

    }
}
