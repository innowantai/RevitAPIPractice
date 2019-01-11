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
    public class LINE
    {
        private XYZ startPoint { get; set; }
        private XYZ endPoint { get; set; }
        private XYZ OriPoint { get; set; }
        private XYZ Direction { get; set; }
        public double c { get; set; } 
        private double Slope { get; set; }
        private double Length { get; set; }
        public string Name { get; set; }
        public string LevelName { get; set; }
        public double Width { get; set; }

        public LINE(XYZ _startPoint, XYZ _endPoint)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Slope = this.GetSlope();
            this.Name = "";
            this.c = this.Slope == -1 ? 0 : this.GetStartPoint().Y - this.Slope * this.GetStartPoint().X;
             SetParaMeters();
        }
        public LINE(XYZ _startPoint, XYZ _endPoint, string NAME)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Name = NAME;
            this.Slope = this.GetSlope();
            this.c = this.Slope == -1 ? 0 : this.GetStartPoint().Y - this.Slope * this.GetStartPoint().X;
             
            SetParaMeters();
        }

        public LINE(XYZ _startPoint, XYZ _endPoint, string NAME, string LevelName_)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Name = NAME;
            this.LevelName = LevelName_;
            this.Slope = this.GetSlope();
            this.c = this.Slope == -1 ? 0 : this.GetStartPoint().Y - this.Slope * this.GetStartPoint().X;

            SetParaMeters();
        }

        public LINE(XYZ _startPoint, XYZ _endPoint, string NAME, string LevelName_, double width_)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Name = NAME;
            this.LevelName = LevelName_;
            this.Width = width_;
            this.Slope = this.GetSlope();
            this.c = this.Slope == -1 ? 0 : this.GetStartPoint().Y - this.Slope * this.GetStartPoint().X;

            SetParaMeters();
        }

        public LINE(XYZ OriPoint_, XYZ Direction_, double Length_)
        {
            this.OriPoint = OriPoint_;
            this.Direction = Direction_;
            this.Length = Length_;
            this.startPoint = OriPoint_;
            this.endPoint = new XYZ(this.OriPoint.X + this.Length * this.Direction.X,
                                    this.OriPoint.Y + this.Length * this.Direction.Y,
                                    this.OriPoint.Z + this.Length * this.Direction.Z);

            this.Slope = this.GetSlope();
            this.c = this.Slope == -1 ? 0 : this.GetStartPoint().Y - this.Slope * this.GetStartPoint().X;
            

        }

        public bool IsPointInLine(XYZ point)
        { 
            double dis1 = this.GetLength(point, this.startPoint);
            double dis2 = this.GetLength(point, this.endPoint);
            if (dis1 > this.GetLength() || dis2 > this.GetLength())
            {
                return false;
            } 
            return true;
        }

        public void ResetParameters(XYZ data, string CASE)
        {
            if (CASE == "OriPoint" || CASE == "StartPoint")
            {
                this.OriPoint = data;
                this.startPoint = data;
            }
            else if (CASE == "EndPoint")
            {
                this.endPoint = data;
            }
            else if (CASE == "Direction")
            {
                this.Direction = data;
            }
            this.Slope = this.GetSlope();
            this.Length = this.GetLength();
            this.c = this.Slope == -1 ? 0 : this.GetStartPoint().Y - this.Slope * this.GetStartPoint().X;
             


            SetParaMeters();
        }

        public void SetParaMeters()
        {
            double dx = this.GetEndPoint().X - this.GetStartPoint().X;
            double dy = this.GetEndPoint().Y - this.GetStartPoint().Y;
            double dz = this.GetEndPoint().Z - this.GetStartPoint().Z;
            this.Length = this.GetLength();
            this.OriPoint = new XYZ(this.GetStartPoint().X, this.GetStartPoint().Y, this.GetStartPoint().Z);

            this.Direction = new XYZ(Math.Round(dx / this.GetLength(),4), Math.Round(dy / this.GetLength(),4), 0);

        }

        public XYZ GetCrossPoint(LINE line2)
        {
            if (this.IsSameDirection(line2.GetDirection(), true))
            {
                LINE tmpLine = new LINE(this.GetEndPoint(),line2.GetStartPoint());
                if (this.IsSameDirection(tmpLine.Direction,true))
                {
                    if (this.IsSameDirection(line2.Direction, true))
                    {
                        return (this.GetEndPoint() + line2.GetStartPoint()) / 2;
                    }
                    else
                    {
                        return (this.GetStartPoint() + line2.GetStartPoint()) / 2;

                    } 
                }
                else
                { 
                    return this.GetEndPoint();  
                }

            }

            MATRIX m1 = new MATRIX(new double[,] { { this.Direction.X, -line2.Direction.X },
                                                    {this.Direction.Y, -line2.Direction.Y } });
            MATRIX m2 = new MATRIX(new double[,] { { line2.OriPoint.X - this.OriPoint.X }, { line2.OriPoint.Y - this.OriPoint.Y } });

            MATRIX m3 = m1.InverseMatrix();
            MATRIX res = m3.CrossMatrix(m2);

            double[,] tt = res.Matrix;
            double newX = this.OriPoint.X + this.Direction.X * tt[0, 0];
            double newY = this.OriPoint.Y + this.Direction.Y * tt[0, 0];

            return new XYZ(newX, newY, this.OriPoint.Z);

        }


        public List<LINE> GetShiftLines(XYZ point, double DD, string CASE)
        {
            LINE[] lines = GetShiftLines(DD);
            double LL1 = lines[0].GetDistanceFromPoint(point);
            double LL2 = lines[1].GetDistanceFromPoint(point);
            Dictionary<string, List<LINE>> allCase = new Dictionary<string, List<LINE>>();
            allCase["IN"] = LL1 > LL2 ? new List<LINE> { lines[1] } : new List<LINE> { lines[0] };
            allCase["OUT"] = LL1 > LL2 ? new List<LINE> { lines[0] } : new List<LINE> { lines[1] };
            allCase["BOTH"] = lines.ToList();

            return allCase[CASE];

        }


        public LINE[] GetShiftLines(double DD)
        {
            if (this.Slope == -1)
            {
                LINE line1 = new LINE(new XYZ(this.GetStartPoint().X + DD, this.GetStartPoint().Y, this.GetStartPoint().Z),
                                      new XYZ(this.GetEndPoint().X + DD, this.GetEndPoint().Y, this.GetEndPoint().Z));
                LINE line2 = new LINE(new XYZ(this.GetStartPoint().X - DD, this.GetStartPoint().Y, this.GetStartPoint().Z),
                                      new XYZ(this.GetEndPoint().X - DD, this.GetEndPoint().Y, this.GetEndPoint().Z));
                return new LINE[] { line1, line2 };
            }
            else if (this.Slope == 0)
            {
                LINE line1 = new LINE(new XYZ(this.GetStartPoint().X, this.GetStartPoint().Y + DD, this.GetStartPoint().Z),
                                      new XYZ(this.GetEndPoint().X, this.GetEndPoint().Y + DD, this.GetEndPoint().Z));
                LINE line2 = new LINE(new XYZ(this.GetStartPoint().X, this.GetStartPoint().Y - DD, this.GetStartPoint().Z),
                                      new XYZ(this.GetEndPoint().X, this.GetEndPoint().Y - DD, this.GetEndPoint().Z));
                return new LINE[] { line1, line2 };
            }
            else
            {
                double _m = -1 / this.Slope;
                double tmpData = DD / Math.Sqrt(_m * _m + 1);
                double newX1 = this.GetStartPoint().X + tmpData;
                double newY1 = this.GetStartPoint().Y + _m * tmpData;
                double newX2 = this.GetStartPoint().X - tmpData;
                double newY2 = this.GetStartPoint().Y - _m * tmpData;

                XYZ newStartPoint1 = new XYZ(newX1, newY1, this.GetStartPoint().Z);
                XYZ newStartPoint2 = new XYZ(newX2, newY2, this.GetStartPoint().Z);
                return new LINE[2] { getNewLine(newStartPoint1), getNewLine(newStartPoint2) };
            }
        }


        public bool IsSameDirection(XYZ dir2,bool IsUseAbs)
        {
            XYZ dir1 = this.Direction;
            if (IsUseAbs && Math.Abs(Math.Round(dir1.X,3)) == Math.Abs(Math.Round(dir2.X, 3)) 
                         && Math.Abs(Math.Round(dir1.Y, 3)) == Math.Abs(Math.Round(dir2.Y, 3))) 
                         //&& Math.Abs(Math.Round(dir1.Z, 3)) == Math.Abs(Math.Round(dir2.Z, 3)))
            {
                return true;
            }
            else if (!IsUseAbs && Math.Round(dir1.X, 3) == Math.Round(dir2.X, 3) 
                               && Math.Round(dir1.Y, 3) == Math.Round(dir2.Y, 3))
                               //&& Math.Round(dir1.Z, 3) == Math.Round(dir2.Z, 3))
            {
                return true; 
            }
            return false; 
        }

        public double GetDistanceFromPoint(XYZ targetPoint)
        {
            double m = this.Slope;
            double b = this.GetStartPoint().Y - m * this.GetStartPoint().X;
            double res = m == -1 ? Math.Abs(targetPoint.X - this.GetStartPoint().X) :
                                   Math.Abs(m * targetPoint.X - targetPoint.Y + b) / Math.Sqrt(m * m + 1);
            return res;
        }

        private LINE getNewLine(XYZ point)
        {
            double m = this.Slope;
            double tmpData = 1 / Math.Sqrt(m * m + 1);
            double newX2 = point.X + this.GetLength() * this.Direction.X;
            double newY2 = point.Y + this.GetLength() * this.Direction.Y;
            return new LINE(point, new XYZ(newX2, newY2, point.Z));
        }

        public double GetSlope()
        {
            double Slope = 0;
            if (this.GetStartPoint().Y == this.GetEndPoint().Y)
            {
                Slope = 0;
            }
            else if (this.GetStartPoint().X == this.GetEndPoint().X)
            {
                Slope = -1;
            }
            else
            {
                Slope = (this.GetStartPoint().Y - this.GetEndPoint().Y) / (this.GetStartPoint().X - this.GetEndPoint().X);
            }
            return Slope;
        }

        public double GetLength()
        {
            return Math.Sqrt(
                (this.GetStartPoint().X - this.GetEndPoint().X) * (this.GetStartPoint().X - this.GetEndPoint().X) +
                (this.GetStartPoint().Y - this.GetEndPoint().Y) * (this.GetStartPoint().Y - this.GetEndPoint().Y)
                + (this.GetStartPoint().Z - this.GetEndPoint().Z) * (this.GetStartPoint().Z - this.GetEndPoint().Z)
                );
        }
        public double GetLength(bool IsPlaneOnly)
        {
            if (IsPlaneOnly)
            { 
                return Math.Sqrt(
                    (this.GetStartPoint().X - this.GetEndPoint().X) * (this.GetStartPoint().X - this.GetEndPoint().X) +
                    (this.GetStartPoint().Y - this.GetEndPoint().Y) * (this.GetStartPoint().Y - this.GetEndPoint().Y) 
                    );
            }
            return Math.Sqrt(
                (this.GetStartPoint().X - this.GetEndPoint().X) * (this.GetStartPoint().X - this.GetEndPoint().X) +
                (this.GetStartPoint().Y - this.GetEndPoint().Y) * (this.GetStartPoint().Y - this.GetEndPoint().Y)
                + (this.GetStartPoint().Z - this.GetEndPoint().Z) * (this.GetStartPoint().Z - this.GetEndPoint().Z)
                );
        }
        private double GetLength(XYZ point1, XYZ point2)
        {
            return Math.Sqrt(
                (point1.X - point2.X) * (point1.X - point2.X) +
                (point1.Y - point2.Y) * (point1.Y - point2.Y)
                + (point1.Z - point2.Z) * (point1.Z - point2.Z)
                );
        }

        public double getSumOfCoordinate()
        {
            return this.GetStartPoint().X + this.GetStartPoint().Y + this.GetEndPoint().X + this.GetEndPoint().Y + this.GetEndPoint().Z + this.GetStartPoint().Z;
        }


        public string ShowPoint()
        {
            string startPoint = "(" + this.GetStartPoint().X.ToString() + " , " + this.GetStartPoint().Y.ToString() + " , " + this.GetStartPoint().Z.ToString() + ")";
            string endPoint = "(" + this.GetEndPoint().X.ToString() + " , " + this.GetEndPoint().Y.ToString() + " , " + this.GetEndPoint().Z.ToString() + ")";
            return startPoint + "   " + endPoint;
        }



        public XYZ GetStartPoint()
        {
            return this.startPoint;
        }

        public XYZ GetEndPoint()
        {
            return this.endPoint;
        }

        public XYZ GetOriPoint()
        {
            return this.OriPoint;
        }
        public XYZ GetDirection()
        {
            return this.Direction;
        }









    }



}
