using Microsoft.Xna.Framework;
using LinAlg = RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.Graphics
{
    class LinAlgConversion
    {
        public static Vector2 XNAVector(LinAlg.Vector2 vec2) => new Vector2(vec2.X, vec2.Y);
    }
}
