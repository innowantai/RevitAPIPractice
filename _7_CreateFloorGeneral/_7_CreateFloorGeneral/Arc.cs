using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace _7_CreateFloorGeneral
{
    class ARC : Geometry
    {

        public ARC(XYZ Center_, double Radius_, XYZ XDirection_, XYZ YDirection_, XYZ Normal_, XYZ StartPoint_, XYZ EndPoint_, bool IsBound_)
        {
            this.Radius = Radius_;
            this.Center = Center_;
            this.XDirection = XDirection_;
            this.YDirection = YDirection_;
            this.Normal = Normal_;
            this.startPoint = StartPoint_;
            this.endPoint = EndPoint_;
            this.IsBound = IsBound_;
            this.IsReverse = false;

            this.StartAngle = GetAngle(this.Center, startPoint);
            this.EndAngle = GetAngle(this.Center, endPoint);

            if (this.StartAngle > this.EndAngle)
            {
                EndAngle += 2*Math.PI;
            }
            double middleAng = (this.EndAngle + this.StartAngle) / 2;
            double newX = this.Center.X + this.Radius * Math.Cos(middleAng);
            double newY = this.Center.Y + this.Radius * Math.Sin(middleAng);
            this.ThirdPoint = new XYZ(newX, newY, this.startPoint.Z);


        }

        

    }
}
