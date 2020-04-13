using System;
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
        public static Lane OptimalLane(Car car, Lane currentLane)
        {
            Lane[] possibleLanes = currentLane.NeighboringLanes;
            VehicleNeighbors currentVehicleNeighbors = currentLane.VehicleNeighbors(car);
            float maxIncentiveCriterion = IncentiveCriterion(
                car, 
                currentVehicleNeighbors, currentVehicleNeighbors,
                currentLane, currentLane);

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
        private static float SafetyCriterion(Car car, Car nextVehicleBehind, Lane nextLane)
        {
            // Compute it's new acceleration based on the longitudinal model
            float nextAccNextVehicleBehind = AccelerationOfCarInLane(nextVehicleBehind, car, nextLane);
            // Make sure it has enough time to brake
            return nextAccNextVehicleBehind - nextVehicleBehind.BrakingDeceleration;
        }

        private static float IncentiveCriterion(
            Car car, 
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
            float nextAccCurrVehicleBehind = AccelerationOfCarInLane(neighborsCurrLane.VehicleBack, car, nextLane);
            float currAccCurrVehicleBehind = AccelerationOfCarInLane(neighborsCurrLane.VehicleBack, neighborsCurrLane.VehicleFront, nextLane);

            // TODO actually legit use this better
            float accThreshold = 0; // IntelligentDriverModel.MIN_ACCELERATION;
            float accBias = 0; // currentLane.AccelerationBias;

            float politenessFactor = car.PolitnessFactor;

            // First term: impact of lane change for current car
            // Second term: impact of lane change on car behind in lane target, weighted with the politeness factor
            // Thrid term: biases for rule enforcing. Acc bias avoids triggering lane changes over marginal gains.
            // Acc bias is used to enfore rules or model obstructions (positive will have cars stay, and vice versa)
            float incentiveCriterion = nextAccCurrentCar - currAccCurrentCar
                - politenessFactor * (currAccCurrVehicleBehind - nextAccCurrVehicleBehind + currAccCurrVehicleBehind - nextAccCurrVehicleBehind)
                + accThreshold + accBias;

            return incentiveCriterion;
        }

        private static float AccelerationOfCarInLane(Car car, Car carInFront, Lane lane)
        {
            if (car == null) return 0;
            LeaderCarInfo leaderCarInfo = lane.ComputeLeaderCarInfo(car, carInFront);
            return IntelligentDriverModel.ComputeAccelerationIntensity(
                car, 
                lane.Path.TangentOfProjectedPosition(car.Position),
                leaderCarInfo.DistToNextCar,
                leaderCarInfo.ApproachingRate
            ).Norm;
        }
    }
}