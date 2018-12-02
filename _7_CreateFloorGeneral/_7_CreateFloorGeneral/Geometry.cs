using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

namespace _7_CreateFloorGeneral
{ 
    public class Geometry
    {
        public string Type { get; set; }
        public XYZ startPoint { get; set; }
        public XYZ endPoint { get; set; }
        public bool IsBound { get; set; }
        public bool IsReverse { get; set; }

        /// ArcUsingProperty
        public XYZ Center { get; set; }
        public double Radius { get; set; }
        public XYZ XDirection { get; set; }
        public XYZ YDirection { get; set; }
        public XYZ Normal { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
        public XYZ ThirdPoint { get; set; }


        public void Reverse()
        {
            XYZ tmp = this.startPoint;
            this.startPoint = this.endPoint;
            this.endPoint = tmp;
            this.IsReverse = true;


             
        }

        public double GetAngle(XYZ centerPoint, XYZ targetPoint)
        {
            XYZ diff = targetPoint - centerPoint;
            double dx = diff.X;
            double dy = diff.Y;
            double ang = 0;
            if (dx == 0)
            {
                ang = dy > 0 ? Math.PI / 2 : Math.PI * 3 / 2;
            }
            else if (dy == 0)
            {
                ang = dx > 0 ? 0 : Math.PI;
            }
            else
            {
                ang = Math.Abs(Math.Atan(dy / dx));
                if (dx < 0 && dy > 0)
                {
                    ang = Math.PI - ang;
                }
                else if (dx < 0 && dy < 0)
                {
                    ang = Math.PI + ang;

                }
                else if (dx > 0 && dy < 0)
                {

                    ang = 2 * Math.PI - ang;
                }
            }

            return ang;
        }
    }
}
