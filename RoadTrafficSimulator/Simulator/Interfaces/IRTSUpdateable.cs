using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadTrafficSimulator.Simulator.Interfaces
{
    interface IRTSUpdateable
    {
        /// <summary>
        /// Update based on time since last update
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void Update(float deltaTime);
    }
}
