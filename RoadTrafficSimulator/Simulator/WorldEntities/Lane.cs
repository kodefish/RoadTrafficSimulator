using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    class Lane
    {
        private Segment midline;
        private List<Car> cars;

        // Think of it like a rail
        public BezierCurve Trajectory { get; private set; }
        public Vector2 Direction { get; private set; }


        public Lane(Segment source, Segment target)
        {
            Segment midline = new Segment(source.Midpoint, target.Midpoint);

            // Trajectoy is just a straight line between the source segment middle to the target segment middle
            Trajectory = new BezierCurve(midline.Source, midline.Source, midline.Target, midline.Target);
            Direction = (target.Midpoint - source.Midpoint).Normalized;

            // Keep track of cars on the lane
            cars = new List<Car>();
        }

        /// <summary>
        /// Returns how far along the lane the position is, 0 being at the source, 1 being at the target
        /// </summary>
        /// <param name="positon">Position in global coordinates</param>
        /// <returns></returns>
        public float GetProgreession(Vector2 positon)
        {
            // check if position is on the line specified by the two points
            if (!midline.PointOnSegment(positon)) throw new ArgumentException("{0} is not in the lane!");
            return Vector2.Distance(midline.Source, positon) / midline.Length;
        }
    }
}
