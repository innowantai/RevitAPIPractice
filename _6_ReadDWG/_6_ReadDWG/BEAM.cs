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

namespace _6_ReadDWG
{
    class BEAM
    {
        private LINE E_LINE_1;
        private LINE E_LINE_2;
        private LINE Center_Line;

        public BEAM(LINE line1 ,LINE line2)
        {
            this.E_LINE_1 = line1;
            this.E_LINE_2 = line2;
            this.Center_Line = GetCenterLine();
        }



        private LINE GetCenterLine()
        {
            XYZ newStPoint = (this.E_LINE_1.GetStartPoint() + this.E_LINE_2.GetStartPoint()) / 2;
            XYZ newEnPoint = (this.E_LINE_1.GetEndPoint() + this.E_LINE_2.GetEndPoint()) / 2;
            LINE centerLine = new LINE(newStPoint, newEnPoint);
            centerLine.Name = this.E_LINE_1.Name;
            centerLine.LevelName = this.E_LINE_1.LevelName;
            centerLine.Width = this.E_LINE_1.Width;

            return centerLine;
        }

        public LINE GetEdgeLine(int target)
        {
            if (target == 1)
            {
                return this.E_LINE_1;
            }
            else if(target == 2)
            {
                return this.E_LINE_2;
            }
            else
            {
                return this.Center_Line;
            }
        }

        


        
    }
}
