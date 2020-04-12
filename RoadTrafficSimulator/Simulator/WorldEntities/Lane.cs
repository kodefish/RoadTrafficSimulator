using System;
using System.Diagnostics;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    struct VehicleNeighbors
    {
        public VehicleNeighbors(Car vehicleBack, Car vehicleFront)
        {
            VehicleBack = vehicleBack;
            VehicleFront = vehicleFront;
        }
        public Car VehicleBack { get; }
        public Car VehicleFront { get; }
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
            get => _neighboringLanes;
            set {
                if (_neighboringLanes != null) 
                    throw new Exception("Neighboring lanes can only be set once!");
                _neighboringLanes = value;
            }
        }    

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
        public List<Car> Cars { get; private set; }
        public Path Path { get; private set; }

        /// <summary>
        /// Contruct a lane between two segments. Default acceleration bias
        /// set to 0.2 as recommended by MOBIL paper, which is classic European rules
        /// </summary>
        public Lane(int laneIdx, float speedLimit, float accelerationBias = 0.2f)
        {
            LaneIdx = laneIdx;
            MaxSpeed = speedLimit;

            // Keep track of cars on the lane
            Cars = new List<Car>();
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

        public void AddCar(Car c)
        {
            Cars.Add(c);
            SortCars();
        }

        public void RemoveCar(Car c)
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

        public LeaderCarInfo ComputeLeaderCarInfo(Car c) => ComputeLeaderCarInfo(c, null);

        public LeaderCarInfo ComputeLeaderCarInfo(Car c1, Car c2)
        {
            float distToNextCar, approachingRate;
            if (c2 == null)
            {
                distToNextCar = Vector2.Distance(
                    c1.Position + c1.Direction * c1.CarLength / 2,
                    Path.PathEnd - Path.TangentOfProjectedPosition(Path.PathEnd) * c1.CarLength / 2); 
                approachingRate = c1.LinearVelocity.Norm;
            }
            else
            {
                distToNextCar = Car.ComputeBumperToBumperVector(c1, c2).Norm;
                approachingRate = Vector2.Distance(c2.LinearVelocity, c1.LinearVelocity);
            }
            return new LeaderCarInfo(distToNextCar, approachingRate); 
        }

        public float FreeLaneSpace()
        {
            if (Cars.Count > 0) return Path.DistanceOfProjectionAlongPath(Cars[0].Position) - Cars[0].CarLength / 2;
            else return Path.Length;
        }


        public VehicleNeighbors VehicleNeighbors(Car car)
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
                i = j - 1;
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
