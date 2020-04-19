using System;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;
using RoadTrafficSimulator.Simulator.WorldEntities;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;

namespace RoadTrafficSimulator.Simulator.DrivingLogic
{
    class Mobil
    {

        /// <summary>
        /// Determines optimal lane using the MOBIL model. 
        /// </summary>
        /// <param name="car">Lane changing car</param>
        /// <param name="currentLane">Car's current lane</param>
        /// <param name="possibleLane">All the possible future lanes</param>
        /// <returns>Optimal lane from possible lanes</returns>
        public static Lane OptimalLane(Vehicle car, Lane currentLane)
        {
            Lane[] possibleLanes = currentLane.NeighboringLanes;
            VehicleNeighbors currentVehicleNeighbors = currentLane.VehicleNeighbors(car);
            float maxIncentiveCriterion = 0;

            // For each possible lane, compute safety and incentive criterions
            // Then, among lanes l s.t. safetyCriterion(l) > 0, pick l s.t. incentiveCriterion(l) is  maximal
            int optimalLaneIdx = -1;
            for (int i = 0; i < possibleLanes.Length; i++)
            {
                Lane potentialNextLane = possibleLanes[i];
                VehicleNeighbors nextVehicleNeighbors = potentialNextLane.VehicleNeighbors(car);
                // If there is no car behidn or it's safe to execute the lane change, check the incentive criterion
                if (nextVehicleNeighbors.VehicleBack == null || SafetyCriterion(car, nextVehicleNeighbors.VehicleBack, potentialNextLane) > 0)
                {
                    float incentiveCriterion = IncentiveCriterion(
                        car, 
                        currentVehicleNeighbors, nextVehicleNeighbors,
                        currentLane, potentialNextLane);

                    if (incentiveCriterion > maxIncentiveCriterion)
                    {
                        maxIncentiveCriterion = incentiveCriterion;
                        optimalLaneIdx = i;
                    }
                }
            }

            // If better alternative is not found, stick to current lane, otherwise change
            return optimalLaneIdx < 0 ? currentLane : possibleLanes[optimalLaneIdx];
        }

        /// <summary>
        /// Computes the safety criterion of a potential lane change. 
        /// The MOBIL safety constraint is satisfied when the safety criterion is > 0
        /// </summary>
        /// <param name="car">Car changing lanes</param>
        /// <param name="nextLane">Car's target lane</param>
        private static float SafetyCriterion(Vehicle car, Vehicle nextVehicleBehind, Lane nextLane)
        {
            // Compute it's new acceleration based on the longitudinal model
            float nextAccNextVehicleBehind = AccelerationOfCarInLane(nextVehicleBehind, car, nextLane);
            // Make sure it has enough time to brake
            return nextAccNextVehicleBehind - nextVehicleBehind.BrakingDeceleration;
        }

        /// <summary>
        /// Computes the incentive criterion of a car, based on itself and it's neighbors. The criterion
        /// is positive if a lane change is beneficial.
        /// </summary>
        /// <param name="car">Car to perform lane change</param>
        /// <param name="neighborsCurrLane">Leading and following vehicles in current lane</param>
        /// <param name="neighborsNextLane">Leading and following vehicles in next lane</param>
        /// <param name="currentLane">Car's current lane</param>
        /// <param name="nextLane">Car's potential next lane</param>
        /// <returns>Incentive criterion.</returns>
        private static float IncentiveCriterion(
            Vehicle car, 
            VehicleNeighbors neighborsCurrLane,
            VehicleNeighbors neighborsNextLane,
            Lane currentLane, 
            Lane nextLane)
        {
            // Accelerations of current car
            float nextAccCurrentCar = AccelerationOfCarInLane(car, neighborsNextLane.VehicleFront, nextLane);
            float currAccCurrentCar = AccelerationOfCarInLane(car, neighborsCurrLane.VehicleFront, currentLane);

            // Check the effects merging between the two cars
            // Accelerations of car behind current car in next lane
            float nextAccNextVehicleBehind = AccelerationOfCarInLane(neighborsNextLane.VehicleBack, car, nextLane);
            float currAccNextVehicleBehind = AccelerationOfCarInLane(neighborsNextLane.VehicleBack, neighborsNextLane.VehicleFront, nextLane);

            // Accelerations of car behind current car in current lane
            float nextAccCurrVehicleBehind = AccelerationOfCarInLane(neighborsCurrLane.VehicleBack, car, currentLane);
            float currAccCurrVehicleBehind = AccelerationOfCarInLane(neighborsCurrLane.VehicleBack, neighborsCurrLane.VehicleFront, currentLane);

            // TODO actually legit use this better
            float accThreshold = IntelligentDriverModel.MIN_ACCELERATION;
            float accBias = currentLane.AccelerationBias;

            float politenessFactor = car.PolitnessFactor;

            // First term: impact of lane change for current car
            // Second term: impact of lane change on car behind in lane target, weighted with the politeness factor
            // Thrid term: biases for rule enforcing. Acc bias avoids triggering lane changes over marginal gains.
            // Acc bias is used to enfore rules or model obstructions (positive will have cars stay, and vice versa)
            float incentiveCriterion = nextAccCurrentCar - currAccCurrentCar
                - politenessFactor * (currAccCurrVehicleBehind - nextAccCurrVehicleBehind + currAccCurrVehicleBehind - nextAccCurrVehicleBehind)
                - accThreshold - accBias;

            return incentiveCriterion;
        }

        /// <summary>
        /// Compute the longitudinal acceleration of a car in a lane, based on the IDM
        /// </summary>
        /// <param name="car">Car to compute acceleration of</param>
        /// <param name="carInFront">Car in front of the current car, may be null if current car is the leader</param>
        /// <param name="lane">Lane the two cars are in</param>
        /// <returns></returns>
        private static float AccelerationOfCarInLane(Vehicle car, Vehicle carInFront, Lane lane)
        {
            if (car == null) return 0;
            LeaderCarInfo leaderCarInfo = lane.ComputeLeaderCarInfo(car, carInFront);
            Vector2 trafficDirection = lane.Path.TangentOfProjectedPosition(car.Position);
            Vector2 acc = IntelligentDriverModel.ComputeAccelerationIntensity(
                car, 
                trafficDirection,
                leaderCarInfo.DistToNextCar,
                leaderCarInfo.ApproachingRate
            );

            // Acceleration in direction of traffic (can be negaitve, -> vehicle will have to brake)
            return Vector2.Dot(acc, trafficDirection); 
        }
    }
}