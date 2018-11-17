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


/// <summary> 
/// Class : 
///     1.CreateObject : The methods that assemble to create Revit's Element or FamilyInstance
///     2.FindRevitElements : Get the information of the revitDoc such all types of the Column and Beam or Levels
///     3.GeometryResult : The methods that used to process Geometry data
///     4.PreProcessing : Get The Geometry Data before CreateObject such the center line of the beams or the center point of the clumns
///     
/// </summary>
/// 
namespace _6_ReadDWG
{
    public class LINE
    {
        public XYZ startPoint { get; set; }
        public XYZ endPoint { get; set; }
        public string Name { get; set; }
        public double Slope { get; set; }
        public double c { get; set; }
        public XYZ OriPoint { get; set; }
        public XYZ Direction { get; set; }
        public LINE(XYZ _startPoint, XYZ _endPoint)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Name = "";
            this.Slope = this.GetSlope();
            this.c = this.Slope == -1 ? 0 : this.startPoint.Y - this.Slope * this.startPoint.X;
            GetParaMeters();
        }
        public LINE(XYZ _startPoint, XYZ _endPoint, string NAME)
        {
            this.startPoint = _startPoint;
            this.endPoint = _endPoint;
            this.Name = NAME;
            GetParaMeters();
        }

        public void GetParaMeters()
        {
            double dx = this.endPoint.X - this.startPoint.X;
            double dy = this.endPoint.Y - this.startPoint.Y;
            double dz = this.endPoint.Z - this.startPoint.Z;
            this.OriPoint = new XYZ(this.startPoint.X, this.startPoint.Y, this.startPoint.Z);
            this.Direction = new XYZ(dx / this.GetLength(), dy / this.GetLength(), 0);
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
                LINE line1 = new LINE(new XYZ(this.startPoint.X + DD, this.startPoint.Y, this.startPoint.Z),
                                      new XYZ(this.endPoint.X + DD, this.endPoint.Y, this.endPoint.Z));
                LINE line2 = new LINE(new XYZ(this.startPoint.X - DD, this.startPoint.Y, this.startPoint.Z),
                                      new XYZ(this.endPoint.X - DD, this.endPoint.Y, this.endPoint.Z));
                return new LINE[] { line1, line2 };
            }
            else if (this.Slope == 0)
            {
                LINE line1 = new LINE(new XYZ(this.startPoint.X, this.startPoint.Y + DD, this.startPoint.Z),
                                      new XYZ(this.endPoint.X, this.endPoint.Y + DD, this.endPoint.Z));
                LINE line2 = new LINE(new XYZ(this.startPoint.X, this.startPoint.Y - DD, this.startPoint.Z),
                                      new XYZ(this.endPoint.X, this.endPoint.Y - DD, this.endPoint.Z));
                return new LINE[] { line1, line2 };
            }
            else
            {
                double _m = -1 / this.Slope;
                double tmpData = DD / Math.Sqrt(_m * _m + 1);
                double newX1 = this.startPoint.X + tmpData;
                double newY1 = this.startPoint.Y + _m * tmpData;
                double newX2 = this.startPoint.X - tmpData;
                double newY2 = this.startPoint.Y - _m * tmpData;

                XYZ newStartPoint1 = new XYZ(newX1, newY1, 0);
                XYZ newStartPoint2 = new XYZ(newX2, newY2, 0);
                return new LINE[2] { getNewLine(newStartPoint1), getNewLine(newStartPoint2) };
            }
        }


        private double GetDistanceFromPoint(XYZ targetPoint)
        {
            double m = this.Slope;
            double b = this.startPoint.Y - m * this.startPoint.X;
            double res = m == -1 ? Math.Abs(targetPoint.X - b) :
                                   Math.Abs(m * targetPoint.X - targetPoint.Y + b) / Math.Sqrt(m * m + 1);
            return res;
        }

        private LINE getNewLine(XYZ point)
        {
            double m = this.Slope;
            double tmpData = 1 / Math.Sqrt(m * m + 1);
            double newX2 = point.X + this.GetLength() * tmpData;
            double newY2 = point.Y + m * this.GetLength() * tmpData;
            return new LINE(point, new XYZ(newX2, newY2, 0));
        }

        public double GetSlope()
        {
            double Slope = 0;
            if (this.startPoint.Y == this.endPoint.Y)
            {
                Slope = 0;
            }
            else if (this.startPoint.X == this.endPoint.X)
            {
                Slope = -1;
            }
            else
            {
                Slope = (this.startPoint.Y - this.endPoint.Y) / (this.startPoint.X - this.endPoint.X);
            }
            return Slope;
        }

        public double GetLength()
        {
            return Math.Sqrt(
                (this.startPoint.X - this.endPoint.X) * (this.startPoint.X - this.endPoint.X) +
                (this.startPoint.Y - this.endPoint.Y) * (this.startPoint.Y - this.endPoint.Y) +
                (this.startPoint.Z - this.endPoint.Z) * (this.startPoint.Z - this.endPoint.Z)
                );
        }

        public double getSumOfCoordinate()
        {
            return this.startPoint.X + this.startPoint.Y + this.startPoint.Z + this.endPoint.X + this.endPoint.Y + this.endPoint.Z;
        }


        public string ShowPoint()
        {
            string startPoint = "(" + this.startPoint.X.ToString() + " , " + this.startPoint.Y.ToString() + " , " + this.startPoint.Z.ToString() + ")";
            string endPoint = "(" + this.endPoint.X.ToString() + " , " + this.endPoint.Y.ToString() + " , " + this.endPoint.Z.ToString() + ")";
            return startPoint + "   " + endPoint;
        }

    }




    /// <summary>
    /// Create Revit Element or Family
    /// </summary>
    public class CreateObjects
    {
        public Document revitDoc;
        /// <summary>
        /// Constructor receive Document of RevitDoc
        /// </summary>
        /// <param name="_revitDoc"></param>
        public CreateObjects(Document _revitDoc)
        {
            this.revitDoc = _revitDoc;
        }

        /// <summary>
        /// Create Column 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="baseLevel"></param>
        /// <param name="topLevel"></param>
        /// <param name="points"></param>
        public void CreateColumn(FamilySymbol Type, Level baseLevel, Level topLevel, LINE points)
        {

            if (!Type.IsActive)
            {
                using (Transaction trans = new Transaction(revitDoc))
                {
                    trans.Start("Activate Family instance");
                    Type.Activate();
                    trans.Commit();
                }

            }
            using (Transaction trans = new Transaction(revitDoc))
            {
                trans.Start("Create Column");
                FamilyInstance familyInstance = null;
                XYZ point = new XYZ((points.startPoint.X + points.endPoint.X) / 2, (points.startPoint.Y + points.endPoint.Y) / 2, 0);
                XYZ botPoint = new XYZ(point.X, point.Y, baseLevel.Elevation);
                XYZ topPoint = new XYZ(point.X, point.Y, topLevel.Elevation);
                familyInstance = revitDoc.Create.NewFamilyInstance(Line.CreateBound(botPoint, topPoint), Type, baseLevel, StructuralType.Column);
                trans.Commit();
            }
        }

        /// <summary>
        /// Create Beam
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="baseLevel"></param>
        /// <param name="points"></param>
        public void CreateBeam(FamilySymbol Type, Level baseLevel, LINE points)
        {
            if (!Type.IsActive)
            {
                using (Transaction trans = new Transaction(revitDoc))
                {
                    trans.Start("Activate Family instance");
                    Type.Activate();
                    trans.Commit();
                }
            }

            using (Transaction trans = new Transaction(revitDoc))
            {

                trans.Start("Create Beam");
                FamilyInstance familyInstance = null;
                XYZ p1 = new XYZ(points.startPoint.X, points.startPoint.Y, baseLevel.Elevation);
                XYZ p2 = new XYZ(points.endPoint.X, points.endPoint.Y, baseLevel.Elevation);
                familyInstance = revitDoc.Create.NewFamilyInstance(Line.CreateBound(p1, p2), Type, baseLevel, StructuralType.Beam);
                

                trans.Commit();
            }
        }




    }

    /// <summary>
    /// Get Revit Document Information
    /// </summary>
    public class FindRevitElements
    {

        /// <summary>
        /// Get Beam Types
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Dictionary<string, List<FamilySymbol>> GetDocBeamTypes(Document doc)
        {
            return this.GetFamilyTypes(doc, BuiltInCategory.OST_StructuralFraming);
        }

        /// <summary>
        /// Get Column Types
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Dictionary<string, List<FamilySymbol>> GetDocColumnsTypes(Document doc)
        {
            return this.GetFamilyTypes(doc, BuiltInCategory.OST_Columns);
        }

        /// <summary>
        /// Get Level
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public List<Level> GetLevels(Document document)
        {

            FilteredElementCollector viewCollector = new FilteredElementCollector(document);
            viewCollector.OfClass(typeof(Level));
            List<Level> ResLevel = new List<Level>();
            foreach (Element viewElement in viewCollector)
            {
                Level ll = (Level)viewElement;
                ResLevel.Add(ll);
            }
            return ResLevel;
        }

        /// <summary>
        /// Get Family Types
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        private Dictionary<string, List<FamilySymbol>> GetFamilyTypes(Document doc, BuiltInCategory cat)
        {
            return new FilteredElementCollector(doc)
                            .WherePasses(new ElementClassFilter(typeof(FamilySymbol)))
                            .WherePasses(new ElementCategoryFilter(cat))
                            .Cast<FamilySymbol>()
                            .GroupBy(e => e.Family.Name)
                            .ToDictionary(e => e.Key, e => e.ToList()); ;
        }

    }

     
    public static class PreProcessing
    {

        /// <summary>
        /// Classify Lines to "closed curve" or "H_dir_lines" or "V_dir_lines" or "else_dir_lines"
        /// </summary>
        /// <param name="LINES"></param>
        /// <param name="Collect"></param>
        /// <param name="H_Direction_Lines"></param>
        /// <param name="V_Direction_Lines"></param>
        /// <param name="Else_Direction_Lines"></param>
        public static void ClassifyLines(List<LINE> LINES,
                                    out List<List<LINE>> Collect,
                                    out List<LINE> H_Direction_Lines,
                                    out List<LINE> V_Direction_Lines,
                                    out List<LINE> Else_Direction_Lines)
        {
            Collect = new List<List<LINE>>();
            int[] is_pickup = new int[LINES.Count];
            for (int i = 0; i < LINES.Count; i++)
            {
                if (is_pickup[i] == 1) continue;

                LINE baseLine = LINES[i];
                List<LINE> tmpData = new List<LINE>();
                tmpData.Add(baseLine);
                int j = 0;

                while (j < LINES.Count)
                {
                    LINE cmpLine = LINES[j];
                    if (is_pickup[j] == 1 || j == i)
                    {
                        j = j + 1;
                        continue;
                    }
                    if (cmpLine.startPoint.X == baseLine.endPoint.X && cmpLine.startPoint.Y == baseLine.endPoint.Y)
                    {
                        baseLine = cmpLine;
                        tmpData.Add(baseLine);
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (cmpLine.endPoint.X == baseLine.endPoint.X && cmpLine.endPoint.Y == baseLine.endPoint.Y)
                    {
                        baseLine = new LINE ( cmpLine.endPoint, cmpLine.startPoint );
                        tmpData.Add(baseLine);
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (tmpData[0].startPoint.X == cmpLine.startPoint.X && tmpData[0].startPoint.Y == cmpLine.startPoint.Y)
                    {
                        tmpData.Insert(0, new LINE ( cmpLine.endPoint, cmpLine.startPoint ));
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (tmpData[0].startPoint.X == cmpLine.endPoint.X && tmpData[0].startPoint.Y == cmpLine.endPoint.Y)
                    {
                        tmpData.Insert(0, cmpLine);
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else
                    {
                        j = j + 1;
                    }
                }

                if (tmpData.Count != 1)
                {
                    Collect.Add(tmpData);
                    is_pickup[i] = 1;
                }
            }

            H_Direction_Lines = new List<LINE>();
            V_Direction_Lines = new List<LINE>();
            Else_Direction_Lines = new List<LINE>();

            for (int ii = 0; ii < is_pickup.Count(); ii++)
            {
                if (is_pickup[ii] == 0)
                {
                    LINE line = LINES[ii];
                    if (GeometryResult.Is_Horizontal(line))
                    {
                        H_Direction_Lines.Add(line);
                    }
                    else if (GeometryResult.Is_Vertical(line))
                    {
                        V_Direction_Lines.Add(line);
                    }
                    else
                    {
                        Else_Direction_Lines.Add(line);
                    }
                }
            }
        }



        /// <summary>
        /// Get Beam parameter Lines using to Create beam those lines is the center line of beam
        /// </summary>
        /// <param name="Collect"></param>
        /// <param name="H_Direction_Lines"></param>
        /// <param name="V_Direction_Lines"></param>
        /// <param name="H_Beams"></param>
        /// <param name="V_Beams"></param>
        public static List<LINE> GetBeamDrawLines(List<List<LINE>> Collect, List<LINE> H_Direction_Lines, List<LINE> V_Direction_Lines)
        {
            List<LINE> RESULT = new List<LINE>(); 

            /// Collect Part
            foreach (List<LINE> co in Collect)
            {
                if (co.Count() <= 4)
                {
                     
                    LINE line1 = co[0];
                    LINE line2 = co[1];
                    XYZ p1 = null;
                    XYZ p2 = null;
                    double ll1 = GeometryResult.Get_Length(line1);
                    double ll2 = GeometryResult.Get_Length(line2);
                    if (ll1 > ll2)
                    {
                        if (line1.startPoint.Y == line1.endPoint.Y)
                        {
                            p1 = new XYZ(line1.startPoint.X, (line1.startPoint.Y + line2.endPoint.Y) / 2, 0);
                            p2 = new XYZ(line1.endPoint.X, (line1.startPoint.Y + line2.endPoint.Y) / 2, 0); 
                        }
                        else if (line1.startPoint.X == line1.endPoint.X)
                        {
                            p1 = new XYZ((line1.startPoint.X + line2.endPoint.X) / 2, line1.startPoint.Y, 0);
                            p2 = new XYZ((line1.startPoint.X + line2.endPoint.X) / 2, line1.endPoint.Y, 0); 
                        }
                    }
                    else
                    {
                        if(line2.startPoint.Y == line2.endPoint.Y)
                        {
                            p1 = new XYZ(line2.startPoint.X, (line1.startPoint.Y + line2.endPoint.Y) / 2, 0);
                            p2 = new XYZ(line2.endPoint.X, (line1.startPoint.Y + line2.endPoint.Y) / 2, 0);

                        }
                        else if (line2.startPoint.X == line2.endPoint.X)
                        {
                            p1 = new XYZ((line1.startPoint.X + line1.endPoint.X) / 2, line2.startPoint.Y, 0);
                            p2 = new XYZ((line1.startPoint.X + line1.endPoint.X) / 2, line2.endPoint.Y, 0); 
                        }
                    }

                    if (p1 != null)
                    {
                        RESULT.Add(new LINE( p1, p2 ));
                    } 
                } 
            }

            /// H-Beam Part
            List<LINE> sorted_H = H_Direction_Lines.OrderBy(e => e.startPoint.Y).ToList();
            List<LINE>  H_Beams = new List<LINE>();
            int[] is_pickup = new int[sorted_H.Count()];
            for (int i = 0; i < sorted_H.Count(); i++)
            {
                if (is_pickup[i] == 1) continue;

                LINE baseLine = sorted_H[i];
                for (int j = 0; j < sorted_H.Count(); j++)
                {
                    if (j == i || is_pickup[j] == 1) continue;

                    LINE cmpLine = sorted_H[j];

                    if (baseLine.GetSlope() == -cmpLine.GetSlope())
                        cmpLine = new LINE(cmpLine.endPoint, cmpLine.startPoint);

                    if (baseLine.startPoint.X == cmpLine.startPoint.X && baseLine.endPoint.X == cmpLine.endPoint.X)
                    {
                        is_pickup[i] = 1;
                        is_pickup[j] = 1;
                        XYZ p1 = new XYZ((baseLine.startPoint.X + cmpLine.startPoint.X) / 2, (baseLine.startPoint.Y + cmpLine.startPoint.Y) / 2, 0);
                        XYZ p2 = new XYZ((baseLine.endPoint.X + cmpLine.endPoint.X) / 2, (baseLine.endPoint.Y + cmpLine.endPoint.Y) / 2, 0);
                        H_Beams.Add(new LINE ( p1, p2 ));
                        RESULT.Add(new LINE ( p1, p2 ));
                        break;
                    }
                }
            }

            /// V-Beam Part
            List<LINE>  V_Beams = new List<LINE>();
            List<LINE> sorted_V = V_Direction_Lines.OrderBy(e => e.startPoint.X).ToList();
            is_pickup = new int[sorted_V.Count()];
            for (int i = 0; i < sorted_V.Count(); i++)
            {
                if (is_pickup[i] == 1) continue;

                LINE baseLine = sorted_V[i];
                for (int j = 0; j < sorted_V.Count(); j++)
                {
                    if (j == i || is_pickup[j] == 1) continue;

                    LINE cmpLine = sorted_V[j];

                    if (baseLine.GetSlope() == - cmpLine.GetSlope()) 
                        cmpLine = new LINE(cmpLine.endPoint, cmpLine.startPoint); 

                    if (baseLine.startPoint.Y == cmpLine.startPoint.Y && baseLine.endPoint.Y == cmpLine.endPoint.Y)
                    {
                        is_pickup[i] = 1;
                        is_pickup[j] = 1;
                        XYZ p1 = new XYZ((baseLine.startPoint.X + cmpLine.startPoint.X) / 2, (baseLine.startPoint.Y + cmpLine.startPoint.Y) / 2, 0);
                        XYZ p2 = new XYZ((baseLine.endPoint.X + cmpLine.endPoint.X) / 2, (baseLine.endPoint.Y + cmpLine.endPoint.Y) / 2, 0);
                        V_Beams.Add(new LINE ( p1, p2 ));
                        RESULT.Add(new LINE ( p1, p2 ));
                        break;
                    }
                }
            }


            return RESULT;
        }



        /// <summary>
        /// Get Column centerpoint using to Create column 
        /// </summary>
        /// <param name="Collect"></param>
        /// <param name="H_Direction_Lines"></param>
        /// <param name="V_Direction_Lines"></param>
        /// <param name="H_Beams"></param>
        /// <param name="V_Beams"></param>
        public static  List<LINE> GetColumnDrawCenterPoints(List<List<LINE>> Collect)
        {

            List<LINE> RESULT = new List<LINE>();
            foreach (List<LINE> Lines in Collect)
            {
                LINE LINE1 = Lines[0];
                LINE LINE2 = Lines[1];
                RESULT.Add(new LINE(LINE1.startPoint,LINE1.endPoint));
            }


            return RESULT;

        }


    }





    /// <summary>
    /// Revit Geometry Processing
    /// </summary>
    public static class GeometryResult
    {

        public static bool Is_Horizontal(LINE line)
        {
            return line.startPoint.Y == line.endPoint.Y ? true : false;
        }

        public static bool Is_Vertical(LINE line)
        {
            return line.startPoint.X == line.endPoint.X ? true : false;
        }

        public static double Get_Length(LINE Line)
        {
             
            return Math.Sqrt(
                (Line.startPoint.X - Line.endPoint.X) * (Line.startPoint.X - Line.endPoint.X) +
                (Line.startPoint.Y - Line.endPoint.Y) * (Line.startPoint.Y - Line.endPoint.Y));
        }
    }
}
