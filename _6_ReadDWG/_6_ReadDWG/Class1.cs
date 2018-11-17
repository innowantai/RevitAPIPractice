using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

/// <summary>
/// 2018/11/17
/// The targets will be solved :
/// 1. If the family instance was not be used, that will appear the message of "familySymbol is not activate" 
///    Solved : Create the Transaction to active indiacted FamilySymbol before use it 
/// 2. Need to get the LevelId of the GeometryInstance
///    Solved : will be ------
/// </summary>


namespace _6_ReadDWG
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Application revitApp;
        public UIDocument uidoc;
        public static Document revitDoc;
        public static string startPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public FindRevitElements RevFind = new FindRevitElements();


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            string getPath = Path.Combine(startPath, "CAD_REVIT_DATA");
            revitDoc = commandData.Application.ActiveUIDocument.Document;
            uidoc = commandData.Application.ActiveUIDocument;
            ElementId ele = null;
            Selection selection = uidoc.Selection;
            ICollection<ElementId> element = selection.GetElementIds();
            foreach (ElementId eleID in element)
            {
                ele = eleID;
                break;
            }


            //CreateBeamsAndColumns Creation = new CreateBeamsAndColumns();
            //Creation.Main_Create(revitDoc, uidoc);



            List<Level> levels = RevFind.GetLevels(revitDoc);
            List<List<LINE>> BeamGroup = GetBeamGroup(levels[0]);
            List<List<LINE>> NewBeamGroup = new List<List<LINE>>();
            foreach (List<LINE> Beams in BeamGroup)
            {
                double sumOfX = Beams.Sum(item => item.startPoint.X);
                double sumOfY = Beams.Sum(item => item.startPoint.Y);
                XYZ tarPoint = new XYZ(sumOfX / Beams.Count, sumOfY / Beams.Count, 0);
                List<LINE> tmpBeams = new List<LINE>();
                foreach (LINE beam in Beams)
                {
                    double width = Convert.ToDouble(beam.Name.Split('x')[0].Trim()) / 304.8;
                    tmpBeams.Add(beam.GetShiftLines(tarPoint, width, "IN")[0]);
                }
                NewBeamGroup.Add(tmpBeams);
            }
             

            foreach (List<LINE> Beams in NewBeamGroup)
            { 
                for (int i = 0; i < Beams.Count; i++)
                {
                    LINE L1 = Beams[i];
                    LINE L2 = i + 1 == Beams.Count ? Beams[0] : Beams[i + 1];
                    L1.endPoint = GetCrossPoint(L1, L2);
                    Beams[i].endPoint = GetCrossPoint(L1, L2);
                    if (i + 1 == Beams.Count)
                    {
                        Beams[0].startPoint = GetCrossPoint(L1, L2); 
                    }
                    else
                    { 
                        Beams[i + 1].startPoint = GetCrossPoint(L1, L2);
                    }
                }
            }

            return Result.Succeeded;
        }

        private XYZ GetCrossPoint(LINE line1, LINE line2)
        {
            if (line1.GetSlope() == line2.GetSlope())
            {
                return line1.endPoint;
            }

            MATRIX m1 = new MATRIX(new double[,] { { line1.Direction.X, -line2.Direction.X },
                                                    {line1.Direction.Y, -line2.Direction.Y } });
            MATRIX m2 = new MATRIX(new double[,] { { line2.OriPoint.X - line1.OriPoint.X }, { line2.OriPoint.Y - line1.OriPoint.Y } });


            MATRIX m3 = m1.InverseMatrix();
            MATRIX res = m3.CrossMatrix(m2);

            double[,] tt = res.Matrix;
            double newX = line1.OriPoint.X + line1.Direction.X * tt[0, 0];
            double newY = line1.OriPoint.Y + line1.Direction.Y * tt[0, 0];


            return new XYZ(newX, newY, 0);

        }

        private List<List<LINE>> GetBeamGroup(Level targetLevel)
        {
            List<LINE> BEAMS = GetTargetFloorBeams(targetLevel);

            int[] count = new int[BEAMS.Count];
            Dictionary<string, List<int>> pickNumbers = new Dictionary<string, List<int>>();
            Dictionary<string, List<LINE>> DictRes = new Dictionary<string, List<LINE>>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                bool open = true;
                List<LINE> Res = new List<LINE>() { BEAMS[i] };
                List<int> checkPicked = new List<int>() { i };
                while (open)
                {
                    int j = 0;
                    List<LINE> tmpRes = new List<LINE>(); tmpRes = Res.ToList();
                    List<int> tmpCheckPicked = new List<int>();
                    while (j < BEAMS.Count)
                    {
                        if (!checkPicked.Contains(j))
                        {
                            if (CMPPoints(Res[Res.Count - 1].endPoint, BEAMS[j].startPoint, 1000 / 304.8))
                            {
                                tmpRes.Add(BEAMS[j]);
                                tmpCheckPicked.Add(j);
                            }
                            else if (CMPPoints(Res[Res.Count - 1].endPoint, BEAMS[j].endPoint, 1000 / 304.8))
                            {
                                tmpRes.Add(new LINE(BEAMS[j].endPoint, BEAMS[j].startPoint, BEAMS[j].Name));
                                tmpCheckPicked.Add(j);
                            }
                        }
                        j++;
                    }

                    if (tmpRes.Count != Res.Count)
                    {
                        LINE tmp = tmpRes[0];
                        List<double> diff = new List<double>();
                        for (int ii = Res.Count; ii < tmpRes.Count; ii++)
                        {
                            diff.Add(new LINE(tmp.startPoint, tmpRes[ii].endPoint).GetLength());
                        }
                        int po = diff.FindIndex(item => item.Equals(diff.Min()));
                        checkPicked.Add(tmpCheckPicked[po]);
                        Res.Add(tmpRes[Res.Count + po]);
                        tmpCheckPicked.Clear();
                        if (diff.Min() < 1000 / 304.8)
                        {
                            open = false;
                        }
                    }
                    else
                    {
                        open = false;
                    }
                }
                pickNumbers[BEAMS[i].Name] = checkPicked;
                DictRes[BEAMS[i].Name] = Res;
            }


            ////// Filter Groups
            List<int> sumOf = new List<int>();
            List<string> keys = pickNumbers.Keys.ToList();
            foreach (KeyValuePair<string, List<int>> item in pickNumbers)
            {
                sumOf.Add(item.Value.Sum());
            }

            int[] flag = new int[sumOf.Count];
            for (int i = 0; i < sumOf.Count; i++)
            {
                if (flag[i] == -1) continue;
                for (int j = 0; j < sumOf.Count; j++)
                {
                    List<LINE> tmpBeams = DictRes[keys[j]];
                    if (i != j && sumOf[i] == sumOf[j])
                    {
                        flag[j] = -1;
                    }
                    else if ((new LINE(tmpBeams[0].startPoint, tmpBeams[tmpBeams.Count - 1].endPoint)).GetLength() > 1000 / 304.8)
                    {
                        flag[j] = -1;
                    }
                }
            }
            List<int> Flag = flag.ToList();
            int[] lastPo = FindAllIndexof(Flag, 0);

            Dictionary<string, List<LINE>> DictResult = new Dictionary<string, List<LINE>>();
            List<List<LINE>> RESULT = new List<List<LINE>>();
            foreach (int pp in lastPo)
            {
                DictResult[keys[pp]] = DictRes[keys[pp]];
                RESULT.Add(DictRes[keys[pp]]);
            }
            return RESULT;
        }


        public static int[] FindAllIndexof<T>(IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }

        /// <summary>
        /// 比較兩點距離是否小於diff
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="diff"></param>
        /// <returns></returns>
        private bool CMPPoints(XYZ point1, XYZ point2, double diff)
        {
            return (new LINE(point1, point2)).GetLength() <= diff ? true : false;
        }






        /// <summary>
        /// Get Beams for indicated floor
        /// </summary>
        /// <param name="targetLevel_"></param>
        /// <returns></returns>
        private List<LINE> GetTargetFloorBeams(Level targetLevel_)
        {
            double targetLevel = targetLevel_.Elevation;
            List<LINE> BEAMS = GetBeamsForIndicatedFloor();

            var beams = from bb in BEAMS
                        where bb.startPoint.Z == targetLevel
                        select bb;

            return beams.ToList();
        }

        /// <summary>
        /// Get All Beams
        /// </summary>
        /// <returns></returns>
        private List<LINE> GetBeamsForIndicatedFloor()
        {
            FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
            collector.OfCategory(BuiltInCategory.OST_StructuralFraming);
            collector.OfClass(typeof(FamilyInstance));
            IList<Element> beams = collector.ToElements();
            List<LINE> Beams = new List<LINE>();
            foreach (Element beam in beams)
            {
                LocationCurve Locurve = beam.Location as LocationCurve;
                Line line = Locurve.Curve as Line;
                LINE LINE = new LINE(line.Origin, new XYZ(line.Origin.X + line.Length * line.Direction.X,
                                                          line.Origin.Y + line.Length * line.Direction.Y,
                                                          line.Origin.Z + line.Length * line.Direction.Z));

                LINE.Name = beam.Name;
                Beams.Add(LINE);
            }



            return Beams;
        }







    }











































    /// <summary>
    /// Allow selection of elements of type T only.
    /// </summary>
    class JtElementsOfClassSelectionFilter<T> : ISelectionFilter where T : Element
    {
        public bool AllowElement(Element e)
        {
            return e is T;
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return true;
        }
    }
}

