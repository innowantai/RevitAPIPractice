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


namespace _6_ReadDWG
{
    class InsertComment
    {

        private string ModifyLayerName = "PARAMETER_FOR_MODIFT_SHIFT";
        public FindRevitElements RevFind = new FindRevitElements();
        private const int IG_POINT = 4;
        public void Main_Create(Document revitDoc, UIDocument uidoc)
        {
            /// 讀取Revit匯入之CAD圖層 
            Dictionary<string, List<LINE>> CADGeometry = null;
            CADGeometry = GeneralCAD(uidoc);
            if (CADGeometry == null) return;

            LINE SHIFT = CADGeometry[ModifyLayerName][0];

            /// 擷取Revit所有樓層資訊
            List<Level> levels = RevFind.GetLevels(revitDoc);

            /// 呼叫Form介面來選擇指定的CAD_output檔案
            Form_InsertCommentToBeam FIC = new Form_InsertCommentToBeam(levels);
            FIC.ShowDialog();

            if (FIC.DialogResult == System.Windows.Forms.DialogResult.OK)
            {

                List<CADGeoObject> DATA_CAD_TEXT = new List<CADGeoObject>();
                List<CADGeoObject> DATA_CAD_GEOM = new List<CADGeoObject>();
                LoadCADOutCSVData(FIC.txtFilePath.Text, ref DATA_CAD_GEOM, ref DATA_CAD_TEXT);


                Form_InsertShowLayers FIC_Layer = new Form_InsertShowLayers(DATA_CAD_TEXT);
                FIC_Layer.ShowDialog();
                if (FIC_Layer.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    List<CADGeoObject> TEXT_DATA = FIC_Layer.Selected_DATA;
                    /// 計算Revit-CAD圖層所有幾何資訊之最小值
                    /// XYZ Revit_CAD_Layer_minPoint = GetRevitImportCADGeometryMiniumn(CADGeometry);
                    /// Revit-CAD 與 CAD 座標偏移計算 
                    List<CADGeoObject> RESULT = ShiftProcessing(DATA_CAD_GEOM, SHIFT.GetStartPoint(), TEXT_DATA);

                    /// 取得所選取之樓層
                    Level targetLevel = FIC.levels[FIC.cmbLevels.SelectedIndex];
                    /// 取得Revit中所選樓層之所有梁物件
                    List<Element> Ele_BEAMS_OUT = new List<Element>();
                    List<LINE> BEAMS = GetTargetFloorBeams(revitDoc, targetLevel, ref Ele_BEAMS_OUT);
                    InsertCommentToBeams(revitDoc, BEAMS, Ele_BEAMS_OUT, RESULT);
                }
            }


        }

        private void InsertCommentToBeams(Document revitDoc, List<LINE> BEAMS, List<Element> Ele_BEAMS_OUT, List<CADGeoObject> RESULT)
        {
            List<int> Text_Po = new List<int>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                List<double> Dist = new List<double>();
                List<int> tmpPo = new List<int>();
                for (int j = 0; j < RESULT.Count; j++)
                {
                    if (Text_Po.Contains(j)) continue;

                    XYZ Point1 = (BEAMS[i].GetStartPoint() + BEAMS[i].GetEndPoint()) / 2;
                    XYZ Point2 = RESULT[j].Point;
                    Dist.Add(GetDistance(Point1, Point2));
                    tmpPo.Add(j);
                }
                int po = FindAllIndexof(Dist, Dist.Min())[0];
                Text_Po.Add(tmpPo[po]);
            }

            List<string> Mark = new List<string>();
            foreach (int ii in Text_Po)
            {
                Mark.Add(RESULT[ii].Text);
            }

            using (Transaction trans = new Transaction(revitDoc, "Set Parameter"))
            {
                trans.Start("Create Floors");
                for (int i = 0; i < Ele_BEAMS_OUT.Count; i++)
                {
                    Element e = Ele_BEAMS_OUT[i];
                    Parameter PP = e.get_Parameter(BuiltInParameter.DOOR_NUMBER);
                    PP.Set(Mark[i]);
                }
                trans.Commit();
            }
            ///
        }


        private double GetDistance(XYZ Point1, XYZ Point2)
        {
            return Math.Sqrt((Point1.X - Point2.X) * (Point1.X - Point2.X) +
                             (Point1.Y - Point2.Y) * (Point1.Y - Point2.Y)
                             );
        }



        /// <summary>
        /// CAD TEXT 座標偏移量處理
        /// </summary>
        /// <param name="DATA_CAD_GEOM"></param>
        /// <param name="RevitCADMin"></param>
        /// <param name="TEXT_DATA"></param>
        /// <returns></returns>
        private List<CADGeoObject> ShiftProcessing(List<CADGeoObject> DATA_CAD_GEOM, XYZ RevitCADMin, List<CADGeoObject> TEXT_DATA)
        {
            //double min_x = DATA_CAD_GEOM.Min(t => t.Point.X);
            //double min_y = DATA_CAD_GEOM.Min(t => t.Point.Y);

            //double DX = min_x - RevitCADMin.X;
            //double DY = min_y - RevitCADMin.Y; 

            double DX = -RevitCADMin.X;
            double DY = -RevitCADMin.Y;


            XYZ SHIFT = new XYZ(DX, DY, 0);
            List<CADGeoObject> RESULT = new List<CADGeoObject>();
            foreach (CADGeoObject item in TEXT_DATA)
            {
                RESULT.Add(new CADGeoObject(item, SHIFT));
            }

            return RESULT.OrderBy(t => t.Point.X).ToList();

        }


        /// <summary>
        /// 取的CAD所有幾何線段之最小值
        /// </summary>
        /// <param name="CADGeometry"></param>
        /// <returns></returns>
        private XYZ GetRevitImportCADGeometryMiniumn(Dictionary<string, List<LINE>> CADGeometry)
        {
            List<LINE> LINE_all = new List<LINE>();
            foreach (KeyValuePair<string, List<LINE>> item in CADGeometry)
            {
                foreach (LINE line in item.Value)
                {
                    LINE_all.Add(line);
                }
            }

            double minX_s = LINE_all.Min(t => t.GetStartPoint().X);
            double minY_s = LINE_all.Min(t => t.GetStartPoint().Y);
            double minX_e = LINE_all.Min(t => t.GetEndPoint().X);
            double minY_e = LINE_all.Min(t => t.GetEndPoint().Y);
            double minX = (minX_s + minX_e) / 2;
            double minY = (minY_s + minY_e) / 2;
            return new XYZ(minX, minY, 0);

        }


        /// <summary>
        /// 篩選指定樓層的梁
        /// </summary>
        /// <param name="targetLevel_"></param>
        /// <returns></returns>
        private List<LINE> GetTargetFloorBeams(Document revitDoc, Level targetLevel_, ref List<Element> Ele_BEAMS_OUT)
        {
            double targetLevel = targetLevel_.Elevation;
            List<Element> Ele_BEAMS = new List<Element>();
            List<LINE> BEAMS = GetAllBeams(revitDoc, targetLevel_, ref Ele_BEAMS);

            int kk = 0;
            List<LINE> Beams_ = new List<LINE>();
            foreach (LINE bb in BEAMS)
            {
                if (bb.LevelName == targetLevel_.Name)
                {
                    Beams_.Add(bb);
                    Ele_BEAMS_OUT.Add(Ele_BEAMS[kk]);
                }
                kk++;
            }

            double minEv = Beams_.Min(t => t.GetStartPoint().Z);

            List<LINE> newBeam = new List<LINE>();
            foreach (LINE item in Beams_)
            {
                XYZ newSt = new XYZ(item.GetStartPoint().X, item.GetStartPoint().Y, minEv);
                XYZ newEn = new XYZ(item.GetEndPoint().X, item.GetEndPoint().Y, minEv);
                LINE newLine = new LINE(newSt, newEn);

                newLine.Name = item.Name;
                newLine.LevelName = item.LevelName;
                newLine.Width = item.Width;
                newBeam.Add(newLine);
            }

            return newBeam;
        }


        /// <summary>
        /// 取得所有的梁(水平與垂直向)
        /// </summary>
        /// <returns></returns>
        private List<LINE> GetAllBeams(Document revitDoc, Level targetLevel_, ref List<Element> Ele_BEAMS)
        {
            FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
            collector.OfCategory(BuiltInCategory.OST_StructuralFraming);
            collector.OfClass(typeof(FamilyInstance));
            IList<Element> beams = collector.ToElements();
            List<LINE> Beams = new List<LINE>();
            foreach (Element beam in beams)
            {
                try
                {

                    ///// 取得梁的Level
                    Parameter mLevel = beam.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
                    string levelName = mLevel.AsValueString();

                    //// 取得梁的類型與其寬度
                    ElementType type = revitDoc.GetElement(beam.GetTypeId()) as ElementType;
                    Parameter b = type.LookupParameter("b");

                    double width = b == null ? 0 : b.AsDouble();



                    ///檢查梁中心線是否有偏移 
                    BuiltInParameter paraIndex = BuiltInParameter.START_Y_JUSTIFICATION;
                    Parameter p1 = beam.get_Parameter(paraIndex);
                    string offset = p1.AsValueString();

                    // 取得梁偏移量
                    BuiltInParameter paraIndex1 = BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION;
                    Parameter of1 = beam.get_Parameter(paraIndex1);
                    string offset1 = of1.AsValueString();
                    BuiltInParameter paraIndex2 = BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION;
                    Parameter of2 = beam.get_Parameter(paraIndex2);
                    string offset2 = of2.AsValueString();

                    if (offset1 != offset2) continue;



                    double shift = Convert.ToDouble(offset1) / 304.8;

                    LocationCurve Locurve = beam.Location as LocationCurve;
                    Line line = Locurve.Curve as Line;

                    XYZ direction = line.Direction;

                    XYZ VertiDir = offset == "左" ? new XYZ(line.Direction.Y, -line.Direction.X, line.Direction.Z) :
                                   offset == "右" ? new XYZ(-line.Direction.Y, line.Direction.X, line.Direction.Z) : line.Direction;


                    XYZ pp1 = line.GetEndPoint(0);
                    XYZ pp2 = line.GetEndPoint(1);

                    ///Ori Points
                    XYZ stPoint = new XYZ(pp1.X + width / 2 * VertiDir.X,
                                          pp1.Y + width / 2 * VertiDir.Y,
                                          pp1.Z + width / 2 * VertiDir.Z);

                    XYZ enPoint = new XYZ(pp1.X + line.Length * line.Direction.X + width / 2 * VertiDir.X,
                                          pp1.Y + line.Length * line.Direction.Y + width / 2 * VertiDir.Y,
                                          pp1.Z + line.Length * line.Direction.Z + width / 2 * VertiDir.Z);

                    LINE LINE = new LINE(stPoint, enPoint);

                    LINE.Name = beam.Name;
                    LINE.LevelName = levelName;
                    LINE.Width = width;


                    Beams.Add(LINE);
                    Ele_BEAMS.Add(beam);

                }
                catch (Exception)
                {

                }

            }
            return Beams;



        }

        /// <summary>
        /// 讀取CAD輸出資料
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private void LoadCADOutCSVData(string filePath, ref List<CADGeoObject> DATA_CAD_GEOM, ref List<CADGeoObject> DATA_CAD_TEXT)
        {
            StreamReader sr = new StreamReader(filePath);
            List<string> tmpData = new List<string>();
            while (sr.Peek() != -1)
            {
                tmpData.Add(sr.ReadLine());
            }


            for (int i = 0; i < tmpData.Count; i++)
            {
                CADGeoObject TmpData = new CADGeoObject(tmpData[i]);
                if (TmpData.Text == null && TmpData.Point != null)
                {
                    DATA_CAD_GEOM.Add(TmpData);
                }
                else if (TmpData.Text != null)
                {
                    DATA_CAD_TEXT.Add(TmpData);

                }
            }
            sr.Close();
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
                ImportInstance import = doc.GetElement(r) as ImportInstance;

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
                            else if (obj is Arc)
                            {  
                                var Arc = obj as Arc;
                                XYZ newPoint = new XYZ(Arc.Center.X, Arc.Center.Y, 0);
                                LINE newLine = new LINE(newPoint, newPoint, 10);
                                newLine.Name = "Circle";
                                tmp.Add(newLine);
                                 

                            }
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
