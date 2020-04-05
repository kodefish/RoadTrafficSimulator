using System;
using System.Collections.Generic;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Simulator.WorldEntities
{
    class Lane
    {
        public const float LANE_WIDTH = 2;    // Width of a lane, in meters

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

        private List<Car> cars;

        // Think of it like a rail
        public Segment Midline { get; private set; }
        public BezierCurve Trajectory { get; private set; }
        public Vector2 Direction => Midline.Direction;

        /// <summary>
        /// Contruct a lane between two segments
        /// </summary>
        public Lane()
        {
            // Keep track of cars on the lane
            cars = new List<Car>();
        }

        private void UpdateMidlineAndTrajectory()
        {
            // Trajectoy is just a straight line between the source segment middle to the target segment middle
            if (SourceSegment != null && TargetSegment != null)
            {
                Midline = new Segment(SourceSegment.Midpoint, TargetSegment.Midpoint);
                Trajectory = new BezierCurve(Midline.Source, Midline.Source, Midline.Target, Midline.Target);
            }
        }

        /// <summary>
        /// Returns how far along the lane the position is, 0 being at the source, 1 being at the target
        /// </summary>
        /// <param name="positon">Position in global coordinates</param>
        /// <returns></returns>
        public float GetProgreession(Vector2 positon)
        {
            // check if position is on the line specified by the two points
            if (!Midline.PointOnSegment(positon)) throw new ArgumentException("{0} is not in the lane!");
            return Vector2.Distance(Midline.Source, positon) / Midline.Length;
        }
    }
}
