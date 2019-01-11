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
    class CreateBeamsAndColumns
    {
        public FindRevitElements RevFind = new FindRevitElements();
        private const int IG_POINT = 4;
        public void Main_Create(Document revitDoc, UIDocument uidoc)
        {
            CreateObjects RevCreate = new CreateObjects(revitDoc);
            Dictionary<string, List<LINE>> res = GeneralCAD(uidoc);

            Dictionary<string, List<LINE>> CADGeometry = null;
            CADGeometry = GeneralCAD(uidoc);
            if (CADGeometry == null) return;

            Dictionary<string, List<FamilySymbol>> colFamilyTypes = RevFind.GetDocColumnsTypes(revitDoc);
            Dictionary<string, List<FamilySymbol>> beamFamilyTypes = RevFind.GetDocBeamTypes(revitDoc);
            List<Level> levels = RevFind.GetLevels(revitDoc);
            Form1 Form = new Form1(colFamilyTypes, beamFamilyTypes, levels, CADGeometry);
            Form.ShowDialog();
            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (Form.chCol.Checked)
                {
                    int CaseIndex = 0;
                    List<LINE> LINES = CADGeometry[Form.cmbColCADLayers.Text];
                    PreProcessing.ClassifyLines(LINES, out List<List<LINE>> Collect,
                                         out List<LINE> H_Direction_Lines,
                                         out List<LINE> V_Direction_Lines,
                                         out List<LINE> Else_Direction_Lines);
                    List<LINE> RES_COLUMN = PreProcessing.GetColumnDrawCenterPoints(Collect);
                    foreach (LINE pp in RES_COLUMN)
                    {
                        RevCreate.CreateColumn(Form.returnType[CaseIndex], Form.returnBaseLevel[CaseIndex], Form.returnTopLevel[CaseIndex], pp);
                    }
                }

                if (Form.chBeam.Checked)
                {
                    int CaseIndex = 1;
                    List<LINE> LINES = CADGeometry[Form.cmbBeamCADLayers.Text];
                    PreProcessing.ClassifyLines(LINES, out List<List<LINE>> Collect,
                                         out List<LINE> H_Direction_Lines,
                                         out List<LINE> V_Direction_Lines,
                                         out List<LINE> Else_Direction_Lines);
                    List<LINE> RES_BEAM = PreProcessing.GetBeamDrawLines(Collect, H_Direction_Lines, V_Direction_Lines);

                    foreach (LINE pp in RES_BEAM)
                    {
                        RevCreate.CreateBeam(Form.returnType[CaseIndex], Form.returnBaseLevel[CaseIndex], pp);
                    }
                }
            }
        }


        public void Create_Version_2(Document revitDoc, UIDocument uidoc)
        {

            CreateObjects RevCreate = new CreateObjects(revitDoc);
            Dictionary<string, List<LINE>> res = GeneralCAD(uidoc);

            Dictionary<string, List<LINE>> BeforeSortedCADGeometry = null;
            /// 讀取CAD所有圖層幾何資訊
            BeforeSortedCADGeometry = GeneralCAD(uidoc);
            /// 若沒選擇CAD 則回傳
            if (BeforeSortedCADGeometry == null) return;



            Dictionary<string, List<LINE>> CADGeometry = new Dictionary<string, List<LINE>>();
            var Layers = BeforeSortedCADGeometry.OrderBy(t => t.Key).ToList();

            foreach (var item in Layers)
            {
                CADGeometry[item.Key] = BeforeSortedCADGeometry[item.Key];
            }

            Dictionary<string, List<FamilySymbol>> colFamilyTypes = RevFind.GetDocColumnsTypes(revitDoc);
            Dictionary<string, List<FamilySymbol>> beamFamilyTypes = RevFind.GetDocBeamTypes(revitDoc);
            List<Level> levels = RevFind.GetLevels(revitDoc);
            Form1 Form = new Form1(colFamilyTypes, beamFamilyTypes, levels, CADGeometry);
            Form.ShowDialog();
            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (Form.chCol.Checked)
                {
                    int CaseIndex = 0;
                    List<LINE> LINES = CADGeometry[Form.cmbColCADLayers.Text];
                    PreProcessing.ClassifyLines(LINES, out List<List<LINE>> Collect,
                                         out List<LINE> H_Direction_Lines,
                                         out List<LINE> V_Direction_Lines,
                                         out List<LINE> Else_Direction_Lines);
                    List<LINE> RES_COLUMN = PreProcessing.GetColumnDrawCenterPoints(Collect);
                    foreach (LINE pp in RES_COLUMN)
                    {
                        RevCreate.CreateColumn(Form.returnType[CaseIndex], Form.returnBaseLevel[CaseIndex], Form.returnTopLevel[CaseIndex], pp);
                    }
                }

                if (Form.chBeam.Checked)
                {
                    List<LINE> LINES = CADGeometry[Form.cmbBeamCADLayers.Text];
                    List<BEAM> Beams = BeamCenterLineProcessing(LINES);

                    int CaseIndex = 1;
                    StartFamilyType(Form.returnType[CaseIndex], revitDoc);
                    CreateBeam(Form.returnType[CaseIndex], Form.returnBaseLevel[CaseIndex], Beams, revitDoc);
                    //foreach (BEAM pp in Beams)
                    //{
                    //    RevCreate.CreateBeam(Form.returnType[CaseIndex], Form.returnBaseLevel[CaseIndex], pp.GetEdgeLine(3));
                    //}
                }
            }
        }



        /// <summary>
        /// 建立梁
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="baseLevel"></param>
        /// <param name="points"></param>
        public void CreateBeam(FamilySymbol Type, Level baseLevel, List<BEAM> points_, Document revitDoc)
        { 

            using (Transaction trans = new Transaction(revitDoc))
            {

                trans.Start("Create Beam");
                foreach (BEAM item in points_)
                {
                    LINE points = item.GetEdgeLine(3);
                    FamilyInstance familyInstance = null;
                    XYZ p1 = new XYZ(points.GetStartPoint().X, points.GetStartPoint().Y, baseLevel.Elevation);
                    XYZ p2 = new XYZ(points.GetEndPoint().X, points.GetEndPoint().Y, baseLevel.Elevation);
                    familyInstance = revitDoc.Create.NewFamilyInstance(Line.CreateBound(p1, p2), Type, baseLevel, StructuralType.Beam); 
                } 
                trans.Commit();
            }
        }

        /// <summary>
        /// 啟動FamilyType
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="revitDoc"></param>
        public void StartFamilyType(FamilySymbol Type, Document revitDoc)
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
        }

        /// <summary>
        /// 梁群組處理
        /// </summary>
        /// <param name="LINES"></param>
        /// <returns></returns>
        private List<BEAM> BeamCenterLineProcessing(List<LINE> LINES)
        {
            List<BEAM> Beams = new List<BEAM>();
            List<int> flag = new List<int>();
            for (int i = 0; i < LINES.Count; i++)
            {
                if (flag.Contains(i)) continue;

                List<int> tmpFlag = new List<int>() { };
                List<LINE> tmpLine = new List<LINE>();
                List<double> tmpDistance = new List<double>();
                for (int j = 0; j < LINES.Count; j++)
                {
                    double dist = 0;
                    if (i == j || tmpFlag.Contains(j) ) continue;
                    bool IsSameDir = IsSameDirection(LINES[i], LINES[j], ref dist);
                    if (IsSameDir && dist != 0)
                    {
                        LINE tmpLine1 = LINES[i];
                        LINE tmpLine2 = LINES[j];
                        bool IsReverse = IsReverseDirection(tmpLine1.GetDirection(), tmpLine2.GetDirection());
                        XYZ stPoint_1; XYZ stPoint_2; XYZ enPoint_1; XYZ enPoint_2;
                        if (IsReverse)
                        {
                            stPoint_1 = tmpLine1.GetStartPoint();
                            stPoint_2 = tmpLine2.GetEndPoint();
                            enPoint_1 = tmpLine1.GetEndPoint();
                            enPoint_2 = tmpLine2.GetStartPoint();
                        }
                        else
                        {
                            stPoint_1 = tmpLine1.GetStartPoint();
                            stPoint_2 = tmpLine2.GetStartPoint();
                            enPoint_1 = tmpLine1.GetEndPoint();
                            enPoint_2 = tmpLine2.GetEndPoint();
                        }
                        double dist_1 = GetDistanceByTwoPoint(stPoint_1, stPoint_2);
                        double dist_2 = GetDistanceByTwoPoint(enPoint_1, enPoint_2);

                        if (dist == dist_1 && dist_1 == dist_2)
                        {
                            LINE newLINE = LINES[j];
                            if (IsReverse)
                            {
                                newLINE = new LINE(LINES[j].GetEndPoint(), LINES[j].GetStartPoint());
                                newLINE.Name = LINES[j].Name;
                                newLINE.LevelName = LINES[j].LevelName;
                                newLINE.Width = LINES[j].Width;
                            }
                            tmpFlag.Add(j);
                            tmpLine.Add(newLINE);
                            tmpDistance.Add(dist);
                        }
                    }
                }

                if (tmpFlag.Count > 0)
                {
                    double minDist = tmpDistance.Min();
                    int po = FindAllIndexof(tmpDistance, minDist)[0];
                    flag.Add(i);
                    flag.Add(tmpFlag[po]);
                    Beams.Add(new BEAM(LINES[i], tmpLine[po]));
                }
            }

            return Beams;
        }

        /// <summary>
        /// 取得兩點距離
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private double GetDistanceByTwoPoint(XYZ point1, XYZ point2)
        { 
            double DX = point1.X - point2.X;
            double DY = point1.Y - point2.Y;
            double DZ =point1.Z - point2.Z;

            return Math.Round(Math.Sqrt(DX * DX + DY * DY + DZ * DZ), IG_POINT);

        }

        /// <summary>
        /// 判斷兩線段是否同向
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool IsSameDirection(LINE line1, LINE line2, ref double distance)
        {
            distance = -1;
            XYZ Dir_1 = line1.GetDirection();
            XYZ Dir_2 = line2.GetDirection();
            bool isSameDirection = IsSamePoint(Dir_1, Dir_2);
            if (!isSameDirection) return false;

            /// 若端點重疊則回傳false
            if (IsSamePoint(line1.GetStartPoint(), line2.GetStartPoint()) ||
                IsSamePoint(line1.GetEndPoint(), line2.GetEndPoint()) ||
                IsSamePoint(line1.GetStartPoint(), line2.GetEndPoint()) ||
                IsSamePoint(line1.GetEndPoint(), line2.GetStartPoint()))
                return false;

            /// 判斷是否在同一直線上 if == 0 
            distance = Math.Round(line1.GetDistanceFromPoint(line2.GetStartPoint()), IG_POINT);

            return true;
        }


        /// <summary>
        /// 判斷兩點是否相同 
        /// </summary>
        /// <param name="Dir_1"></param>
        /// <param name="Dir_2"></param>
        /// <param name="isVerseDir"></param>
        /// <returns></returns>
        private bool IsSamePoint(XYZ Dir_1, XYZ Dir_2)
        {
            XYZ DIR_1 = new XYZ(Math.Round(Dir_1.X, 4), Math.Round(Dir_1.Y, 4), Math.Round(Dir_1.Z, 4));
            XYZ DIR_2 = new XYZ(Math.Round(Dir_2.X, 4), Math.Round(Dir_2.Y, 4), Math.Round(Dir_2.Z, 4));

            if (Math.Abs(DIR_1.X) == Math.Abs(DIR_2.X) && Math.Abs(DIR_1.Y) == Math.Abs(DIR_2.Y) && Math.Abs(DIR_1.Z) == Math.Abs(DIR_2.Z))
            {
                return true;
            }

            return false;

        }


        /// <summary>
        /// 判斷方向是否相反
        /// </summary>
        /// <param name="Dir_1"></param>
        /// <param name="Dir_2"></param>
        /// <returns></returns>
        private bool IsReverseDirection(XYZ Dir_1, XYZ Dir_2)
        {
            XYZ DIR_1 = new XYZ(Math.Round(Dir_1.X, 4), Math.Round(Dir_1.Y, 4), Math.Round(Dir_1.Z, 4));
            XYZ DIR_2 = new XYZ(Math.Round(Dir_2.X, 4), Math.Round(Dir_2.Y, 4), Math.Round(Dir_2.Z, 4));

            if (DIR_1.X == DIR_2.X && DIR_1.Y == DIR_2.Y && DIR_1.Z == DIR_2.Z)
            {
                return false;

            }
            else if (DIR_1.X == -DIR_2.X && DIR_1.Y == -DIR_2.Y && DIR_1.Z == DIR_2.Z)
            {
                return true;

            }
            else if (-DIR_1.X == DIR_2.X && -DIR_1.Y == DIR_2.Y && DIR_1.Z == DIR_2.Z)
            {
                return true;
            }

            return false;

        }


        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public Dictionary<string, List<LINE>> GeneralCAD(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View active_view = doc.ActiveView;
            List<GeometryObject> visible_dwg_geo = new List<GeometryObject>();

            try
            { 
                // Pick Import Instance 
                Reference r = uidoc.Selection.PickObject(ObjectType.Element, new JtElementsOfClassSelectionFilter<ImportInstance>());
                var import = doc.GetElement(r) as ImportInstance;

                // Get Geometry 
                var ge = import.get_Geometry(new Options());
                foreach (var go in ge)
                {
                    if (go is GeometryInstance)
                    {
                        var gi = go as GeometryInstance;
                        var ge2 = gi.GetInstanceGeometry();
                        if (ge2 != null)
                        {
                            foreach (var obj in ge2)
                            {
                                // Only work on PolyLines 
                                if (obj is PolyLine | obj is Arc | obj is Line)
                                {

                                    // Use the GraphicsStyle to get the 
                                    // DWG layer linked to the Category 
                                    // for visibility.

                                    var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                                    // Check if the layer is visible in the view.

                                    if (!active_view.GetCategoryHidden(gStyle.GraphicsStyleCategory.Id))
                                    {
                                        visible_dwg_geo.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }


                // Do something with the info 
                Dictionary<string, List<LINE>> res = new Dictionary<string, List<LINE>>();
                if (visible_dwg_geo.Count > 0)
                {
                    // Retrieve first filled region type 
                    var filledType = new FilteredElementCollector(doc)
                      .WhereElementIsElementType()
                      .OfClass(typeof(FilledRegionType))
                      .OfType<FilledRegionType>()
                      .First();

                    using (var t = new Transaction(doc))
                    {
                        t.Start("ProcessDWG");
                        List<LINE> tmp = new List<LINE>();
                        foreach (var obj in visible_dwg_geo)
                        {
                            var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                            string layerName = gStyle.GraphicsStyleCategory.Name;

                            tmp = res.ContainsKey(layerName) ? res[layerName] : new List<LINE>();
                            if (obj is PolyLine)
                            {
                                // Create loops for detail region 
                                var poly = obj as PolyLine;
                                var points = poly.GetCoordinates();
                                for (int kk = 0; kk < points.Count - 1; kk++)
                                {
                                    try
                                    {
                                        tmp.Add(new LINE(points[kk], points[kk + 1]));
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            else if (obj is Line)
                            {
                                var line = obj as Line;
                                tmp.Add(new LINE(line.GetEndPoint(0),
                                                     line.GetEndPoint(1)));
                            }
                            //else if (obj is Arc)
                            //{
                            //    var Arc = obj as Arc;
                            //    pp[0] = Arc.Center;
                            //    pp[1] = Arc.Center;
                            //} 
                            res[layerName] = tmp;
                        }
                    }
                }



                return res;
            }
            catch (Exception)
            {

                return null;
            }
        }


        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public Dictionary<string, List<LINE>> ProcessVisible(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View active_view = doc.ActiveView;
            List<GeometryObject> visible_dwg_geo = new List<GeometryObject>();

            // Pick Import Instance 
            Reference r = uidoc.Selection.PickObject(ObjectType.Element, new JtElementsOfClassSelectionFilter<ImportInstance>());
            var import = doc.GetElement(r) as ImportInstance;

            // Get Geometry 
            var ge = import.get_Geometry(new Options());
            foreach (var go in ge)
            {
                if (go is GeometryInstance)
                {
                    var gi = go as GeometryInstance;
                    var ge2 = gi.GetInstanceGeometry();
                    if (ge2 != null)
                    {
                        foreach (var obj in ge2)
                        {
                            // Only work on PolyLines 
                            if (obj is PolyLine | obj is Arc | obj is Line)
                            {

                                // Use the GraphicsStyle to get the 
                                // DWG layer linked to the Category 
                                // for visibility.

                                var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                                // Check if the layer is visible in the view.

                                if (!active_view.GetCategoryHidden(gStyle.GraphicsStyleCategory.Id))
                                {
                                    visible_dwg_geo.Add(obj);
                                }
                            }
                        }
                    }
                }
            }

            // Do something with the info 
            Dictionary<string, List<LINE>> res = new Dictionary<string, List<LINE>>();
            if (visible_dwg_geo.Count > 0)
            {
                // Retrieve first filled region type 
                var filledType = new FilteredElementCollector(doc)
                  .WhereElementIsElementType()
                  .OfClass(typeof(FilledRegionType))
                  .OfType<FilledRegionType>()
                  .First();

                using (var t = new Transaction(doc))
                {
                    t.Start("ProcessDWG");
                    List<LINE> tmp = new List<LINE>();
                    foreach (var obj in visible_dwg_geo)
                    {
                        var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                        string layerName = gStyle.GraphicsStyleCategory.Name;
                        LINE pp = null;
                        if (obj is PolyLine)
                        {
                            // Create loops for detail region 
                            var poly = obj as PolyLine;
                            var points = poly.GetCoordinates();
                            pp = new LINE(points[0], points[2]);

                        }
                        else if (obj is Arc)
                        {
                            var Arc = obj as Arc;
                            pp = new LINE(Arc.Center, Arc.Center);
                        }
                        else if (obj is Line)
                        {
                            var line = obj as Line;
                            pp = new LINE(line.GetEndPoint(0), line.GetEndPoint(1));
                        }
                        tmp = res.ContainsKey(layerName) ? res[layerName] : new List<LINE>();
                        tmp.Add(pp);
                        res[layerName] = tmp;
                    }
                }
            }

            return res;
        }


        public static int[] FindAllIndexof<T>(IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }

    }
}
