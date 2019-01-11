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
    public class CADGeoObject
    {
        private string Data;
        public string Layer;
        public string Text;
        public XYZ Point_;
        public XYZ Point;

        public CADGeoObject(string Data_)
        {
            this.Data = Data_;
            this.Text = null;
            this.Layer = null;
            this.Point_ = null;
            this.Point_ = null;
            try
            {
                Process(this.Data);
                this.Point = new XYZ(this.Point_.X / 304.8, this.Point_.Y / 304.8, 0);

            }
            catch (Exception)
            {
            }
        }

        public CADGeoObject(CADGeoObject oriData, XYZ SHIFT)
        {
            this.Data = null;
            this.Layer = oriData.Layer;
            this.Text = oriData.Text;
            this.Point = new XYZ(oriData.Point.X - SHIFT.X, oriData.Point.Y - SHIFT.Y, 0);

        }


        private void Process(string Data)
        {
            string[] tmpData = Data.Split(',');
            this.Layer = tmpData[0].Trim();
            this.Text = null;
            if (tmpData[1].Trim() == "Line" || tmpData[1].Trim() == "Polyline")
            {
                string[] Point1 = tmpData[3].Replace("(", "").Replace(")", "").Split(' ');
                string[] Point2 = tmpData[4].Replace("(", "").Replace(")", "").Split(' ');
                this.Point_ = new XYZ((Convert.ToDouble(Point1[0]) + Convert.ToDouble(Point2[0])) / 2,
                                     (Convert.ToDouble(Point1[1]) + Convert.ToDouble(Point2[1])) / 2,
                                     (Convert.ToDouble(Point1[2]) + Convert.ToDouble(Point2[2])) / 2);

            }
            else if (tmpData[1].Trim() == "Circle" || tmpData[1].Trim() == "Arc")
            {
                //string[] Point1 = tmpData[5].Replace("(", "").Replace(")", "").Split(' ');
                //this.Point_ = new XYZ(Convert.ToDouble(Point1[0]),
                //                     Convert.ToDouble(Point1[1]),
                //                     Convert.ToDouble(Point1[2]));

            }
            else
            {
                string[] Point1 = tmpData[3].Replace("(", "").Replace(")", "").Split(' ');
                this.Point_ = new XYZ(Convert.ToDouble(Point1[0]),
                                     Convert.ToDouble(Point1[1]),
                                     Convert.ToDouble(Point1[2]));
                this.Text = tmpData[2];

            }
        }

    }
}
