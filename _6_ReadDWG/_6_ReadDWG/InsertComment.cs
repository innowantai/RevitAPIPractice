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

        private const int IG_POINT = 4;
        private string ModifyLayerName = "PARAMETER_FOR_MODIFT_SHIFT";
        public FindRevitElements RevFind = new FindRevitElements();
        public void Main_Create(Document revitDoc, UIDocument uidoc)
        {


            /// 建立CAD處理物件
            GetCADImformation GetCADImformation = new GetCADImformation(true, true, true);
            GetCADImformation.CADProcessing(uidoc);

            /// 讀取Revit匯入之CAD圖層 
            Dictionary<string, List<LINE>> CADGeometry = GetCADImformation.LayersAndGeometries;

            //if (CADGeometry == null) return;
            //LINE SHIFT2 = CADGeometry[ModifyLayerName][0];

            XYZ SHIFT = GetCADImformation.Origin;
            double Rotation = GetCADImformation.Rotation;

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
                    /// Revit-CAD 與 CAD 座標偏移計算 
                    List<CADGeoObject> RESULT = ShiftProcessing(TEXT_DATA, SHIFT, Rotation);
                    List<CADGeoObject> GeoRESULT = ShiftProcessing(DATA_CAD_GEOM, SHIFT, Rotation);

                    /// 取得所選取之樓層  
                    string Message = "";
                    foreach (Level targetLevel in FIC.OUT_SelectedLevels)
                    {
                        string mess = "";
                        if (FIC.cmbFamilyType.SelectedIndex == 0)
                        {
                            /// 取得Revit中所選樓層之所有梁物件
                            List<Element> Ele_BEAMS_OUT = new List<Element>();
                            List<Element> Ele_BEAMS_OUT_ = new List<Element>();
                            List<LINE> BEAMS_ = GetTargetFloorBeams(revitDoc, targetLevel, ref Ele_BEAMS_OUT_);
                            List<LINE> BEAMS = FilterObjectInRange(BEAMS_, GeoRESULT, Ele_BEAMS_OUT_, ref Ele_BEAMS_OUT);
                            mess = InsertCommentToBeams(revitDoc, BEAMS, Ele_BEAMS_OUT, RESULT, FIC.cmbFamilyType.SelectedIndex);
                        }
                        else if (FIC.cmbFamilyType.SelectedIndex == 1)
                        {
                            List<Element> Ele_COLUMNs_OUT_ = new List<Element>();
                            List<Element> Ele_COLUMNs_OUT = new List<Element>();
                            List<LINE> Columns_ = GetAllColumnsCenterByLocation(revitDoc, targetLevel, ref Ele_COLUMNs_OUT_);
                            List<LINE> Columns = FilterObjectInRange(Columns_, GeoRESULT, Ele_COLUMNs_OUT_, ref Ele_COLUMNs_OUT);
                            mess = InsertCommentToBeams(revitDoc, Columns, Ele_COLUMNs_OUT, RESULT, FIC.cmbFamilyType.SelectedIndex);

                        }
                        Message += "第" + targetLevel.Name + "層," + mess;
                    }

                    System.Windows.Forms.MessageBox.Show(Message);
                }
            }
        }

        private List<LINE> FilterObjectInRange(List<LINE> BEAMS, List<CADGeoObject> GeoRESULT, List<Element> Ele_COLUMNs_OUT_, ref List<Element> Ele_COLUMNs_OUT)
        { 
            int kk = 0;
            List<LINE> RESULT = new List<LINE>();
            foreach (LINE item in BEAMS)
            { 
                    RESULT.Add(item);
                    Ele_COLUMNs_OUT.Add(Ele_COLUMNs_OUT_[kk]); 
                kk++;
            }

            return RESULT;
        }


        private List<LINE> FilterObjectInRange_Range(List<LINE> BEAMS, List<CADGeoObject> GeoRESULT, List<Element> Ele_COLUMNs_OUT_, ref List<Element> Ele_COLUMNs_OUT)
        {
            double min_x = GeoRESULT.Min(t => t.Point.X);
            double min_y = GeoRESULT.Min(t => t.Point.Y);
            double max_x = GeoRESULT.Max(t => t.Point.X);
            double max_y = GeoRESULT.Max(t => t.Point.Y);
            int kk = 0;
            List<LINE> RESULT = new List<LINE>();
            foreach (LINE item in BEAMS)
            {
                if (item.GetStartPoint().X > min_x && item.GetStartPoint().Y > min_y &&
                    item.GetStartPoint().X < max_x && item.GetStartPoint().Y < max_y)
                {
                    RESULT.Add(item);
                    Ele_COLUMNs_OUT.Add(Ele_COLUMNs_OUT_[kk]);
                }
                kk++;
            }

            return RESULT;
        }


        /// <summary>
        /// 插入標簽到物件中
        /// </summary>
        /// <param name="revitDoc"></param>
        /// <param name="BEAMS"></param>
        /// <param name="Ele_BEAMS_OUT"></param>
        /// <param name="RESULT"></param>
        /// <param name="CASE"></param>
        private string InsertCommentToBeams(Document revitDoc, List<LINE> BEAMS, List<Element> Ele_BEAMS_OUT, List<CADGeoObject> RESULT, int CASE)
        { 
            List<int> Text_Po = new List<int>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                List<double> Dist = new List<double>();
                List<int> tmpPo = new List<int>();
                for (int j = 0; j < RESULT.Count; j++)
                {
                    if (Text_Po.Contains(j)) continue;

                    XYZ Point1 = CASE == 0 ? (BEAMS[i].GetStartPoint() + BEAMS[i].GetEndPoint()) / 2 :
                                              BEAMS[i].GetStartPoint();
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

            // System.Windows.Forms.MessageBox.Show(Mark.Count.ToString());

            return "處理" + Mark.Count.ToString() + "個物件  \r\n";
        }

        /// <summary>
        /// 計算兩點平面距離
        /// </summary>
        /// <param name="Point1"></param>
        /// <param name="Point2"></param>
        /// <returns></returns>
        private double GetDistance(XYZ Point1, XYZ Point2)
        {
            return Math.Sqrt((Point1.X - Point2.X) * (Point1.X - Point2.X) +
                             (Point1.Y - Point2.Y) * (Point1.Y - Point2.Y)
                             );
        }

        /// <summary>
        /// CAD TEXT 座標偏移量處理 , Will Add Rotation Property
        /// </summary>
        /// <param name="DATA_CAD_GEOM"></param>
        /// <param name="RevitCADMin"></param>
        /// <param name="TEXT_DATA"></param>
        /// <returns></returns>
        private List<CADGeoObject> ShiftProcessing(List<CADGeoObject> TEXT_DATA, XYZ RevitCADMin, double Rotation)
        {

            double DX = RevitCADMin.X;
            double DY = RevitCADMin.Y;

            XYZ SHIFT = new XYZ(DX, DY, 0);
            List<CADGeoObject> RESULT = new List<CADGeoObject>();
            foreach (CADGeoObject item in TEXT_DATA)
            {
                RESULT.Add(new CADGeoObject(item, SHIFT, Rotation));
            }

            return RESULT.OrderBy(t => t.Point.X).ToList();

        }


        /// <summary>
        /// 取得所有的Column
        /// </summary>
        /// <returns></returns>
        private List<LINE> GetAllColumnsCenterByLocation(Document revitDoc, Level targetLevel, ref List<Element> Ele_COLUMNs_OUT)
        {


            FilteredElementCollector collector = new FilteredElementCollector(revitDoc);

            collector.OfCategory(BuiltInCategory.OST_StructuralColumns);
            collector.OfClass(typeof(FamilyInstance));

            IList<Element> columns = collector.ToElements();

            List<LINE> ColumnsCenter = new List<LINE>();
            foreach (FamilyInstance familyOfColumn in columns)
            {
                Level cLevel = revitDoc.GetElement(familyOfColumn.LevelId) as Level;
                if (cLevel.Name == targetLevel.Name)
                {
                    Element ele = familyOfColumn as Element;
                    Ele_COLUMNs_OUT.Add(ele);
                    LocationPoint LP = familyOfColumn.Location as LocationPoint;
                    XYZ pp = new XYZ(LP.Point.X, LP.Point.Y, 0);
                    ColumnsCenter.Add(new LINE(pp, pp, 1));
                }
            }
            return ColumnsCenter;
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


        public static int[] FindAllIndexof<T>(IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }



        ///// <summary>
        ///// 取得所有的Column
        ///// </summary>
        ///// <returns></returns>
        //private List<LINE> GetAllColumnsBoundary(Document revitDoc, Level targetLevel, ref List<Element> Ele_COLUMNs_OUT)
        //{
        //    FilteredElementCollector collector = new FilteredElementCollector(revitDoc);

        //    collector.OfCategory(BuiltInCategory.OST_StructuralColumns);
        //    collector.OfClass(typeof(FamilyInstance));

        //    IList<Element> columns = collector.ToElements();

        //    Dictionary<string, List<LINE>> ResultColumns = new Dictionary<string, List<LINE>>();
        //    List<LINE> ColumnsCenter = new List<LINE>();
        //    int kk = 0;
        //    foreach (FamilyInstance familyOfColumn in columns)
        //    {
        //        Level cLevel = revitDoc.GetElement(familyOfColumn.LevelId) as Level;
        //        if (cLevel.Name == targetLevel.Name)
        //        {
        //            CurveLoop curve = GetFaceEdgelines(familyOfColumn);
        //            List<LINE> Curves = new List<LINE>();
        //            string Type = "";
        //            foreach (Curve cc in curve)
        //            {
        //                if (cc.GetType().Name == "Line")
        //                {
        //                    Type = "Line";
        //                    Line Line = cc as Line;
        //                    Curves.Add(new LINE(Line.Origin, Line.Direction, Line.Length));
        //                }
        //            }
        //            double mean_x = Curves.Sum(t => t.GetStartPoint().X) / Curves.Count;
        //            double mean_y = Curves.Sum(t => t.GetStartPoint().Y) / Curves.Count;
        //            XYZ pp = new XYZ(mean_x, mean_y, 0);
        //            ColumnsCenter.Add(new LINE(pp, pp, 1));
        //            //ResultColumns[kk.ToString() + " " + Type] = Curves;
        //            Element ele = familyOfColumn as Element;
        //            Ele_COLUMNs_OUT.Add(ele);
        //            kk++;
        //        }
        //    }
        //    return ColumnsCenter;
        //}


        ///// <summary>
        ///// 幾何 -> 固體 -> 表面 -> 邊
        ///// Geomerty -> Solid -> surface -> edges
        ///// </summary>
        ///// <param name="famulyOfColumns"></param>
        //private CurveLoop GetFaceEdgelines(FamilyInstance famulyOfColumns)
        //{
        //    CurveLoop curveLoop = new CurveLoop();
        //    Options opt = new Options();
        //    opt.ComputeReferences = true;
        //    opt.DetailLevel = ViewDetailLevel.Fine;
        //    GeometryElement geomElem = famulyOfColumns.get_Geometry(opt);
        //    if (geomElem != null)
        //    {
        //        /// GeometryElement 包含三個枚舉(GeometryInstance, Solid, Solid)
        //        foreach (var item in geomElem)
        //        {
        //            /// 取得Type為Solid的枚舉
        //            if (item.GetType().Name == "Solid")
        //            {
        //                /// 轉換至Solid型態
        //                Solid solid = item as Solid;

        //                /// 若無面，略過
        //                if (solid.Faces.Size == 0) continue;

        //                /// 列舉Solid所有面
        //                foreach (Face face in solid.Faces)
        //                {
        //                    /// 轉換至Surface型態
        //                    Surface surface = face.GetSurface();

        //                    /// 若為平面 else 圓柱面
        //                    if (face.GetType().Name == "PlanarFace")
        //                    {
        //                        Plane plane = surface as Plane;
        //                        /// 取得基準面
        //                        if (Math.Round(plane.Normal.X, 2) == 0 && Math.Round(plane.Normal.Y, 2) == 0 && Math.Round(plane.Normal.Z, 2) == -1)
        //                        {
        //                            curveLoop = face.GetEdgesAsCurveLoops()[0];
        //                        }
        //                    }
        //                    else if (face.GetType().Name == "CylindricalFace")
        //                    {
        //                        CylindricalSurface csf = surface as CylindricalSurface;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                GeometryInstance GI = item as GeometryInstance;
        //                curveLoop = GetEdgeSoildProcessing(GI);
        //            }
        //        }
        //    }

        //    return curveLoop;
        //}

        //private CurveLoop GetEdgeSoildProcessing(GeometryInstance geomElem)
        //{
        //    CurveLoop curveLoop = new CurveLoop();
        //    foreach (var item in geomElem.SymbolGeometry)
        //    {
        //        /// 取得Type為Solid的枚舉
        //        if (item.GetType().Name == "Solid")
        //        {
        //            /// 轉換至Solid型態
        //            Solid solid = item as Solid;

        //            /// 若無面，略過
        //            if (solid.Faces.Size == 0) continue;

        //            /// 列舉Solid所有面
        //            foreach (Face face in solid.Faces)
        //            {
        //                /// 轉換至Surface型態
        //                Surface surface = face.GetSurface();

        //                /// 若為平面 else 圓柱面
        //                if (face.GetType().Name == "PlanarFace")
        //                {
        //                    Plane plane = surface as Plane;
        //                    /// 取得基準面
        //                    if (Math.Round(plane.Normal.X, 2) == 0 && Math.Round(plane.Normal.Y, 2) == 0 && Math.Round(plane.Normal.Z, 2) == -1)
        //                    {
        //                        curveLoop = face.GetEdgesAsCurveLoops()[0];
        //                    }
        //                }
        //                else if (face.GetType().Name == "CylindricalFace")
        //                {
        //                    CylindricalSurface csf = surface as CylindricalSurface;
        //                }
        //            }
        //        }
        //    }

        //    return curveLoop;

        //}

    }



}
