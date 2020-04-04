using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadTrafficSimulator.Interfaces;
using RoadTrafficSimulator.DataStructures;

namespace RoadTrafficSimulator.XNAHelpers
{
    class Intersection : IRTSPosition
    {
        public Vector2 Position { get; set; }

        public Intersection(Vector2 position)
        {
            this.Position = position;
        }
    }
}
