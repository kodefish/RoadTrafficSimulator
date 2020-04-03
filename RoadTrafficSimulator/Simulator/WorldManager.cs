using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadTrafficSimulator.Simulator
{
    // Acts as a bridge between RoadTrafficSimulator and Monogame
    class WorldManager
    {

        private SimulatorWorld world;

        public WorldManager()
        {
            world = new SimulatorWorld();
        }
    }
}
