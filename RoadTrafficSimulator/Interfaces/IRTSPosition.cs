using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadTrafficSimulator.DataStructures;

namespace RoadTrafficSimulator.Interfaces
{
    interface IRTSPosition
    {
        /// <summary>
        /// Get the position of the object, to place it in the RTS simulation world
        /// </summary>
        /// <returns></returns>
        Vector2 Position { get; set; }
    }
}
