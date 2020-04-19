﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DrivingLogic;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    struct VehicleNeighbors
    {
        public VehicleNeighbors(Vehicle vehicleBack, Vehicle vehicleFront)
        {
            VehicleBack = vehicleBack;
            VehicleFront = vehicleFront;
        }
        public Vehicle VehicleBack { get; }
        public Vehicle VehicleFront { get; }
    }
    class Lane : IRTSUpdateable
    {
        // Lane params
        public const float LANE_WIDTH = 3;      // Width of a lane, in meters
        public int LaneIdx;                     // Lane index in lane[] of parent road
        public float MaxSpeed { get; }          // Speedlimit
        public float AccelerationBias { get; }  // Used in MOBIL, can be used to enforce various rules
        private Lane[] _neighboringLanes;
        public Lane[] NeighboringLanes {        // Neighboring lanes (left and right) in road
            get => _neighboringLanes == null ? new Lane[0] : _neighboringLanes;
            set {
                if (_neighboringLanes != null) 
                    throw new Exception("Neighboring lanes can only be set once!");
                _neighboringLanes = value;
            }
        }    

        private readonly FourWayIntersection TargetIntersection;
        private Lane _nextLane;
        public Lane NextLane {
            get {
                if (_nextLane == null) return TargetIntersection.NextLane(this);
                else return _nextLane;
            }
        }

        // Lane geometry
        private Segment _sourceSegment;
        private Segment _targetSegment;
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
        public List<Vehicle> Cars { get; private set; }
        public Path Path { get; private set; }

        /// <summary>
        /// Contruct a lane between two segments. Default acceleration bias
        /// set to 0.2 as recommended by MOBIL paper, which is classic European rules
        /// </summary>
        public Lane(int laneIdx, float speedLimit, FourWayIntersection targetIntersection, float accelerationBias = 0.2f)
        {
            LaneIdx = laneIdx;
            MaxSpeed = speedLimit;
            AccelerationBias = accelerationBias;
            TargetIntersection = targetIntersection;

            // Keep track of cars on the lane
            Cars = new List<Vehicle>();
        }

        /// <summary>
        /// Contruct a lane between two segments. Default acceleration bias
        /// set to 0.2 as recommended by MOBIL paper, which is classic European rules
        /// </summary>
        public Lane(int laneIdx, float speedLimit, Lane nextLane, float accelerationBias = 0.2f)
        {
            LaneIdx = laneIdx;
            MaxSpeed = speedLimit;
            AccelerationBias = accelerationBias;
            _nextLane = nextLane;            

            // Keep track of cars on the lane
            Cars = new List<Vehicle>();
        }

        private void UpdateMidlineAndTrajectory()
        {
            // Trajectoy is just a straight line between the source segment middle to the target segment middle
            if (SourceSegment != null && TargetSegment != null)
            {
                Vector2 source = SourceSegment.Midpoint;
                Vector2 target = TargetSegment.Midpoint;
                /*
                float dist = Vector2.Distance(source, target) / 2;
                BezierCurve midlineCurve = new BezierCurve(
                    source, source + SourceSegment.Direction.Normal * dist,
                    target, target - TargetSegment.Direction.Normal * dist);
                Path = Path.FromBezierCurve(midlineCurve, 2, LANE_WIDTH / 2);
                */
                List<Segment> segments = new List<Segment>();
                segments.Add(new Segment(source, target));
                Path = new Path(segments, LANE_WIDTH / 2);
            }
        }

        public void AddCar(Vehicle c)
        {
            Cars.Add(c);
            SortCars();
        }

        public void RemoveCar(Vehicle c)
        {
            Cars.Remove(c);
            SortCars();
        }

        private void SortCars()
        {
            // Sort cars by distance from path start
            Cars.Sort((a, b) => Path.DistanceOfProjectionAlongPath(a.Position).CompareTo(Path.DistanceOfProjectionAlongPath(b.Position)));
        }

        public void Update(float deltaTime)
        {
            if (Cars.Count == 0) return;

            // For all the cars that have a car in front of them
            for (int i = 0; i < Cars.Count - 1; i++)
            {
                Cars[i].DrivingState.SetLeaderCarInfo(LaneIdx, ComputeLeaderCarInfo(Cars[i], Cars[i + 1]));
            }

            // Car that is at the head of the lane
            // TODO do some fancy shit with the next lane based on intersection lights
            // For noew, Just set the end of the lane
            Cars[Cars.Count - 1].DrivingState.SetLeaderCarInfo(LaneIdx, ComputeLeaderCarInfo(Cars[Cars.Count - 1]));
        }

        public LeaderCarInfo ComputeLeaderCarInfo(Vehicle c) => ComputeLeaderCarInfo(c, null);

        public LeaderCarInfo ComputeLeaderCarInfo(Vehicle c1, Vehicle c2)
        {
            float distToNextCar, approachingRate;
            if (c2 == null)
            {
                Lane nextLane = c1.DrivingState.NextLane;
                if (nextLane == null)
                {
                    // If no next lane is available distance to next car is pretending there a car in the intersection that is min bumper distance away
                    // so the IDM will make the front of the car touch the end of the lane. Since the origin is in the middle
                    // of the car, we offset that distance by half the car length
                    distToNextCar = Vector2.Distance(
                        c1.Position,
                        Path.PathEnd + TargetSegment.Direction.Normal * (IntelligentDriverModel.MIN_BUMPER_TO_BUMPER_DISTANCE - c1.VehicleLength / 2)); 
                    approachingRate = c1.LinearVelocity.Norm;
                }
                else
                {
                    // If there is a next lane, the distance is the distance to the end of the current lane + the distance to the first car
                    // in the next lane
                    distToNextCar = Vector2.Distance(c1.Position, Path.PathEnd) + nextLane.FreeLaneSpace();
                    approachingRate = c1.LinearVelocity.Norm - (nextLane.Cars.Count > 0 ? nextLane.Cars[0].LinearVelocity.Norm : 0);
                }
            }
            else
            {
                distToNextCar = Vehicle.ComputeBumperToBumperVector(c1, c2).Norm;
                approachingRate = Vector2.Distance(c2.LinearVelocity, c1.LinearVelocity);
            }
            return new LeaderCarInfo(distToNextCar, approachingRate); 
        }

        public float FreeLaneSpace()
        {
            if (Cars.Count > 0) return Path.DistanceOfProjectionAlongPath(Cars[0].Position) - Cars[0].VehicleLength / 2;
            else return Path.Length;
        }


        public VehicleNeighbors VehicleNeighbors(Vehicle car)
        {
            // Assume cars are sorted and binary search for least index j s.t. InvLerp(j) > InvLerp(c)
            float carVal = Path.InverseLerp(car.Position);

            int firstGreaterThanIdx = -1;
            int low = 0, high = Cars.Count - 1;
            while (low <= high)
            {
                int m = low + (high - low + 1) / 2;
                float mVal = Path.InverseLerp(Cars[m].Position);

                if (mVal <= carVal)
                {
                    low = m + 1;
                }
                else 
                {
                    firstGreaterThanIdx = m;
                    high = m - 1;
                }
            }

            // If car comes before all the cars (BS finds 0), then i -> null, j -> 0
            // If car comes after all the cars (BS finds -1), then i -> n - 1, j -> null
            // Else j = BS result and i = j - 1
            int i = -1, j = -1;
            if (firstGreaterThanIdx == 0) 
            {
                j = firstGreaterThanIdx;
            }
            else if (firstGreaterThanIdx < 0)
            {
                i = Cars.Count - 1;
            }
            else 
            {
                j = firstGreaterThanIdx;
                i = Cars.Contains(car) ? j - 2 : j - 1;
            }

            // Check if there is a vehicle at the same position as c
            if (i > 0 && Cars[i].Position.Equals(car.Position))
                return new VehicleNeighbors(Cars[i], Cars[i]);
            else if (j > 0 && Cars[j].Position.Equals(car.Position)) // Should absolutely never happen, but ey you never know and better be robust
                return new VehicleNeighbors(Cars[j], Cars[j]); // If you feeling fancy, throw an exception that something went horribly wrong
            else
                return new VehicleNeighbors(
                    i < 0 ? null : Cars[i], 
                    j < 0 ? null : Cars[j]
                );
        }

    }
}
