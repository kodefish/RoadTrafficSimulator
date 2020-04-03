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
        private Vector2 position;

        public Intersection(Vector2 position)
        {
            this.position = position;
        }

        public Vector2 GetGlobalPosition()
        {
            return position;
        }
    }
}
