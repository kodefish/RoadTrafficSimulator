using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadTrafficSimulator.DataStructures
{
    class BezierCurve
    {
        private readonly Vector2 supportPoint1, supportPoint2, supportPoint3, supportPoint4;

        public BezierCurve(Vector2 supportPoint1,Vector2 supportPoint2,Vector2 supportPoint3,Vector2 supportPoint4)
        {
            this.supportPoint1 = supportPoint1;
            this.supportPoint2 = supportPoint2;
            this.supportPoint3 = supportPoint3;
            this.supportPoint4 = supportPoint4;
        }

        public Vector2 GetPosition(float step)
        {
            throw new NotImplementedException(); 
        }
    }
}
