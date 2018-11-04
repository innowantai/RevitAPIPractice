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
        public void CreateColumn(FamilySymbol Type, Level baseLevel, Level topLevel, XYZ[] points)
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
                XYZ point = new XYZ((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2, 0);
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
        public void CreateBeam(FamilySymbol Type, Level baseLevel, XYZ[] points)
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
                XYZ p1 = new XYZ(points[0].X, points[0].Y, baseLevel.Elevation);
                XYZ p2 = new XYZ(points[1].X, points[1].Y, baseLevel.Elevation);
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
        public static void ClassifyLines(List<XYZ[]> LINES,
                                    out List<List<XYZ[]>> Collect,
                                    out List<XYZ[]> H_Direction_Lines,
                                    out List<XYZ[]> V_Direction_Lines,
                                    out List<XYZ[]> Else_Direction_Lines)
        {
            Collect = new List<List<XYZ[]>>();
            int[] is_pickup = new int[LINES.Count];
            for (int i = 0; i < LINES.Count; i++)
            {
                if (is_pickup[i] == 1) continue;

                XYZ[] baseLine = LINES[i];
                List<XYZ[]> tmpData = new List<XYZ[]>();
                tmpData.Add(baseLine);
                int j = 0;

                while (j < LINES.Count)
                {
                    XYZ[] cmpLine = LINES[j];
                    if (is_pickup[j] == 1 || j == i)
                    {
                        j = j + 1;
                        continue;
                    }
                    if (cmpLine[0].X == baseLine[1].X && cmpLine[0].Y == baseLine[1].Y)
                    {
                        baseLine = cmpLine;
                        tmpData.Add(baseLine);
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (cmpLine[1].X == baseLine[1].X && cmpLine[1].Y == baseLine[1].Y)
                    {
                        baseLine = new XYZ[] { cmpLine[1], cmpLine[0] };
                        tmpData.Add(baseLine);
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (tmpData[0][0].X == cmpLine[0].X && tmpData[0][0].Y == cmpLine[0].Y)
                    {
                        tmpData.Insert(0, new XYZ[] { cmpLine[1], cmpLine[0] });
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (tmpData[0][0].X == cmpLine[1].X && tmpData[0][0].Y == cmpLine[1].Y)
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
             
            H_Direction_Lines = new List<XYZ[]>();
            V_Direction_Lines = new List<XYZ[]>();
            Else_Direction_Lines = new List<XYZ[]>();

            for (int ii = 0; ii < is_pickup.Count(); ii++)
            {
                if (is_pickup[ii] == 0)
                {
                    XYZ[] line = LINES[ii];
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
        /// Get beam parameter Lines using to Create beam those lines is the center line of beam
        /// </summary>
        /// <param name="Collect"></param>
        /// <param name="H_Direction_Lines"></param>
        /// <param name="V_Direction_Lines"></param>
        /// <param name="H_Beams"></param>
        /// <param name="V_Beams"></param>
        public static void BeamDrawLinesProcess(List<List<XYZ[]>> Collect, 
                                                List<XYZ[]> H_Direction_Lines,
                                                List<XYZ[]> V_Direction_Lines,
                                                out List<XYZ[]> H_Beams,
                                                out List<XYZ[]> V_Beams)
        {
            List<XYZ[]> sorted_H = H_Direction_Lines.OrderBy(e => e[0].Y).ToList();
            H_Beams = new List<XYZ[]>();
            int[] is_pickup = new int[sorted_H.Count()];
            for (int i = 0; i < sorted_H.Count(); i++)
            {
                if (is_pickup[i] == 1) continue;

                XYZ[] baseLine = sorted_H[i];
                for (int j = 0; j < sorted_H.Count(); j++)
                {
                    if (j == i || is_pickup[j] == 1) continue;

                    XYZ[] cmpLine = sorted_H[j];
                    if (baseLine[0].X == cmpLine[0].X && baseLine[1].X == cmpLine[1].X)
                    {
                        is_pickup[i] = 1;
                        is_pickup[j] = 1;
                        XYZ p1 = new XYZ((baseLine[0].X + cmpLine[0].X)/2, (baseLine[0].Y + cmpLine[0].Y)/2, 0);
                        XYZ p2 = new XYZ((baseLine[1].X + cmpLine[1].X)/2, (baseLine[1].Y + cmpLine[1].Y)/2, 0);
                        H_Beams.Add(new XYZ[] { p1, p2 });
                        break;
                    }
                }
            }


            V_Beams = new List<XYZ[]>();
            List<XYZ[]> sorted_V = V_Direction_Lines.OrderBy(e => e[0].X).ToList();
            is_pickup = new int[sorted_V.Count()];
            for (int i = 0; i < sorted_V.Count(); i++)
            {
                if (is_pickup[i] == 1) continue;

                XYZ[] baseLine = sorted_V[i];
                for (int j = 0; j < sorted_V.Count(); j++)
                {
                    if (j == i || is_pickup[j] == 1) continue;

                    XYZ[] cmpLine = sorted_V[j];
                    if (baseLine[0].Y == cmpLine[0].Y && baseLine[1].Y == cmpLine[1].Y)
                    {
                        is_pickup[i] = 1;
                        is_pickup[j] = 1;
                        XYZ p1 = new XYZ((baseLine[0].X + cmpLine[0].X) / 2, (baseLine[0].Y + cmpLine[0].Y) / 2, 0);
                        XYZ p2 = new XYZ((baseLine[1].X + cmpLine[1].X) / 2, (baseLine[1].Y + cmpLine[1].Y) / 2, 0);
                        V_Beams.Add(new XYZ[] { p1,p2});
                        break;
                    }
                }
            }


        }



    }





    /// <summary>
    /// Revit Geometry Processing
    /// </summary>
    public static class GeometryResult
    { 
        public static bool Is_Horizontal(XYZ[] line)
        { 
            return line[0].Y == line[1].Y  ? true : false;
        }

        public static bool Is_Vertical(XYZ[] line)
        {
            return line[0].X == line[1].X  ? true : false;
        }
         
    }
}
