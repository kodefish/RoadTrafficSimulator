using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadTrafficSimulator.DataStructures;

namespace RoadTrafficSimulator.Interfaces
{
    interface IRTSDimension
    {
        /// <summary>
        /// Get the dimensions of an object, width and height (assumus everything to be rectangles)
        /// </summary>
        /// <returns></returns>
        Vector2 GetDimensions();
    }
}
