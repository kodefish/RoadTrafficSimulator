using System;
using System.Diagnostics;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.Interfaces;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.DrivingLogic;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    /// <summary>
    /// Struct represents two vehicles, front and back of some other vehicle that may or may not be in the same lane
    /// </summary>
    struct VehicleNeighbors
    {
        public VehicleNeighbors(Vehicle vehicleBack, Vehicle vehicleFront)
        {
            VehicleBack = vehicleBack;
            VehicleFront = vehicleFront;
        }

        /// <summary>
        /// Vehicle behind
        /// </summary>
        public Vehicle VehicleBack { get; }

        /// <summary>
        /// Vehicle in front
        /// </summary>
        public Vehicle VehicleFront { get; }
    }

    /// <summary>
    /// Represents a lane in a road.
    /// </summary>
    class Lane : IRTSUpdateable
    {
        // Lane params
        /// <summary>
        /// Width of a lane, in meters
        /// </summary>
        public const float LANE_WIDTH = 3;

        /// <summary>
        /// Lane index in lane[] of parent road
        /// </summary>
        public int LaneIdx;

        /// <summary>
        /// Speed limit in current lane, in [m/s]
        /// </summary>
        public float MaxSpeed { get; }

        /// <summary>
        /// Used in MOBIL, can be used to enforce various rules. A positive bias incentivises
        /// vehicles to stay in the lane, and a negative one to leave.
        /// </summary>
        public float AccelerationBias { get; }

        private Lane[] _neighboringLanes;
        /// <summary>
        /// Neighboring lanes (left and right) in road. Can only be set once.
        /// </summary>
        public Lane[] NeighboringLanes {
            get => _neighboringLanes == null ? new Lane[0] : _neighboringLanes;
            set {
                if (_neighboringLanes != null) 
                    throw new Exception("Neighboring lanes can only be set once!");
                _neighboringLanes = value;
            }
        }    

        /// <summary>
        /// Reference to target intersection
        /// </summary>
        private readonly FourWayIntersection TargetIntersection;

        /// <summary>
        /// If null, means lane has a dynamic next lane (based on target intersection)
        /// </summary>
        private Lane _nextLane;

        /// <summary>
        /// Lane to take after the current lane's end is reached. Can either be
        /// set in the contructor when it's fixed (e.g. an intersection lane's 
        /// next lane will always be available) or dynamic (a road lane's next 
        /// lane may be null if the upcoming traffic light is red).
        /// </summary>
        public Lane NextLane {
            get {
                if (_nextLane == null) return TargetIntersection.NextLane(this);
                else return _nextLane;
            }
        }

        // Lane geometry
        private Segment _sourceSegment;
        private Segment _targetSegment;

        /// <summary>
        /// Source segment of the lane
        /// </summary>
        public Segment SourceSegment {
            get => _sourceSegment;
            set {
                _sourceSegment = value;
                UpdatePath();
            }
        }

        /// <summary>
        /// Target segment of the lane
        /// </summary>
        public Segment TargetSegment {
            get => _targetSegment;
            set {
                _targetSegment = value;
                UpdatePath();
            }
        }

        // Lane logic
        /// <summary>
        /// List of vehicles in the lane
        /// </summary>
        public List<Vehicle> Vehicles { get; private set; }

        /// <summary>
        /// Path corresponding to the midline of the lane
        /// </summary>
        public Path Path { get; private set; }

        /// <summary>
        /// Contruct a lane between two segments. Default acceleration bias
        /// set to 0.2 as recommended by MOBIL paper, which is classic European rules. 
        /// In this case, the lane's next lane is dynamic, based on target intersection's
        /// traffic light state
        /// </summary>
        /// <param name="laneIdx">Index of the lane</param>
        /// <param name="speedLimit">Speed limit, in m/s</param>
        /// <param name="targetIntersection">Target intersection</param>
        /// <param name="accelerationBias">Acceleration bias</param>
        public Lane(int laneIdx, float speedLimit, FourWayIntersection targetIntersection, float accelerationBias = 0.2f)
        {
            LaneIdx = laneIdx;
            MaxSpeed = speedLimit;
            AccelerationBias = accelerationBias;
            TargetIntersection = targetIntersection;

            // Keep track of cars on the lane
            Vehicles = new List<Vehicle>();
        }

        /// <summary>
        /// Contruct a lane between two segments. Default acceleration bias
        /// set to 0.2 as recommended by MOBIL paper, which is classic European rules. 
        /// In this case, the lane's next lane is static, based on turn direction and
        /// source segment (computed in Intersection)
        /// </summary>
        /// <param name="laneIdx">Index of the lane</param>
        /// <param name="speedLimit">Speed limit, in m/s</param>
        /// <param name="nextLane">Next lane to follow</param>
        /// <param name="accelerationBias">Acceleration bias</param>
        public Lane(int laneIdx, float speedLimit, Lane nextLane, float accelerationBias = 0.2f)
        {
            LaneIdx = laneIdx;
            MaxSpeed = speedLimit;
            AccelerationBias = accelerationBias;
            _nextLane = nextLane;            

            // Keep track of cars on the lane
            Vehicles = new List<Vehicle>();
        }

        /// <summary>
        /// Updates the path of the midline of the lane
        /// </summary>
        private void UpdatePath()
        {
            // Trajectoy is just a straight line between the source segment middle to the target segment middle
            if (SourceSegment != null && TargetSegment != null)
            {
                Vector2 source = SourceSegment.Midpoint;
                Vector2 target = TargetSegment.Midpoint;
                List<Segment> segments = new List<Segment>();
                segments.Add(new Segment(source, target));
                Path = new Path(segments, LANE_WIDTH / 2);
            }
        }

        /// <summary>
        /// Add a vehicle to the lane
        /// </summary>
        /// <param name="v">Vehicle to add</param>
        public void AddVehicle(Vehicle v)
        {
            Vehicles.Add(v);

            // Only need to sort vehicles when adding/removing 
            // since when overtaking, a vehicle will leave the lane
            SortVehicles();
        }

        /// <summary>
        /// Remove vehicle from a lane (vehicle has left the lane)
        /// </summary>
        /// <param name="v">Vehicle leaving the lane</param>
        public void RemoveVehicle(Vehicle v)
        {
            Vehicles.Remove(v);
            // Only need to sort vehicles when adding/removing 
            // since when overtaking, a vehicle will leave the lane
            SortVehicles();
        }

        /// <summary>
        /// Sort the vehicles from nearest to furthest from the source segment
        /// </summary>
        private void SortVehicles()
        {
            // Sort cars by distance from path start
            Vehicles.Sort((a, b) => Path.DistanceOfProjectionAlongPath(a.Position).CompareTo(Path.DistanceOfProjectionAlongPath(b.Position)));
        }

        /// <summary>
        /// Update the lane's vehicles leader car information
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // No vehicles, no problem
            if (Vehicles.Count == 0) return;

            // For all the cars that have a car in front of them, update the leading car info
            for (int i = 0; i < Vehicles.Count - 1; i++)
            {
                Vehicles[i].DrivingState.SetLeaderVehicleInfo(LaneIdx, ComputeLeaderCarInfo(Vehicles[i], Vehicles[i + 1]));
            }

            // Car that is at the head of the lane has no leader, so distance to next car
            // will either be distance to end of the lane if the next lane is unavailable (red light)
            // or distance to end of the lane + distance to the first vehicle in the next lane
            Vehicles[Vehicles.Count - 1].DrivingState.SetLeaderVehicleInfo(LaneIdx, ComputeLeaderVehicleInfo(Vehicles[Vehicles.Count - 1]));
        }

        /// <summary>
        /// Compute leader vehicle info for vehicle closest to end of the lane
        /// </summary>
        /// <param name="v">Leading vehicle</param>
        public LeaderVehicleInfo ComputeLeaderVehicleInfo(Vehicle v) => ComputeLeaderCarInfo(v, null);

        /// <summary>
        /// Compute distance and approaching rate between two cars
        /// </summary>
        public LeaderVehicleInfo ComputeLeaderCarInfo(Vehicle c1, Vehicle c2)
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
                    approachingRate = c1.LinearVelocity.Norm - (nextLane.Vehicles.Count > 0 ? nextLane.Vehicles[0].LinearVelocity.Norm : 0);
                }
            }
            else
            {
                distToNextCar = Vehicle.ComputeBumperToBumperVector(c1, c2).Norm;
                approachingRate = Vector2.Distance(c2.LinearVelocity, c1.LinearVelocity);
            }
            return new LeaderVehicleInfo(distToNextCar, approachingRate); 
        }

        /// <summary>
        /// Space until first vehicle in the lane, starting from the source.
        /// </summary>
        public float FreeLaneSpace()
        {
            if (Vehicles.Count > 0) return Path.DistanceOfProjectionAlongPath(Vehicles[0].Position) - Vehicles[0].VehicleLength / 2;
            else return Path.Length;
        }

        /// <summary>
        /// Computes the neighbors (front and back) of a vehicle. If the vehicle is not in the lane,
        /// the projected position of the car is taken. Complexity is log(n) since we use binary search.
        /// </summary>
        public VehicleNeighbors VehicleNeighbors(Vehicle car)
        {
            // Assume cars are sorted and binary search for least index j s.t. InvLerp(j) > InvLerp(c)
            float carVal = Path.InverseLerp(car.Position);

            int firstGreaterThanIdx = -1;
            int low = 0, high = Vehicles.Count - 1;
            while (low <= high)
            {
                int m = low + (high - low + 1) / 2;
                float mVal = Path.InverseLerp(Vehicles[m].Position);

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
                i = Vehicles.Count - 1;
            }
            else 
            {
                j = firstGreaterThanIdx;
                // If the vehicle is in the lane, then j-1 will equal the vehicle, so take j - 2 to get the car behind
                i = Vehicles.Contains(car) ? j - 2 : j - 1;
            }

            // Check if there is a vehicle at the same position as c
            if (i > 0 && Vehicles[i].Position.Equals(car.Position))
                return new VehicleNeighbors(Vehicles[i], Vehicles[i]);
            else if (j > 0 && Vehicles[j].Position.Equals(car.Position)) // Should absolutely never happen, but ey you never know and better be robust
                return new VehicleNeighbors(Vehicles[j], Vehicles[j]); // If you feeling fancy, throw an exception that something went horribly wrong
            else
                return new VehicleNeighbors(
                    i < 0 ? null : Vehicles[i], 
                    j < 0 ? null : Vehicles[j]
                );
        }

    }
}
