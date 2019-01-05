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
    class CreateFloors
    {
        private Document revitDoc;

        public CreateFloors(Document revitDoc_)
        {
            this.revitDoc = revitDoc_;
        }

        public void CreateFloor(Level targetLevel, FloorType floor_type)
        {

            /// 取得目標樓層所有的梁
            List<LINE> BEAMS = GetTargetFloorBeams(targetLevel);
            List<List<LINE>> BEAMS_Class = ClassBeamsByEvelution(BEAMS);
            foreach (var BB in BEAMS_Class)
            { 
                List<List<LINE>> Result = this.DataPreProcessing(targetLevel, BB);
                this.CreateFloor(targetLevel, Result, floor_type);
            }

            /////////////////// Test Part ////////////////
            //FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
            //collector.OfCategory(BuiltInCategory.OST_StructuralFraming);
            //collector.OfClass(typeof(FamilyInstance));
            //IList<Element> beams = collector.ToElements();
            //Element BEAM = beams[0];



            //Parameter mLevel = BEAM.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
             

        }

        private List<List<LINE>> DataPreProcessing(Level targetLevel,List<LINE> BEAMs)
        {
             

            /// 將所有的梁轉換至各樓板邊緣線
            List<List<LINE>> BeamGroup = GetBeamGroup(BEAMs);

            /// 將樓板邊緣線偏移並連接
            List<List<LINE>> NewBeamGroup = BeamConnectAndShiftProcessing(BeamGroup);

            /// 取得目標樓層所有的柱
            Dictionary<string, List<LINE>> columns = GetAllColumnsBoundary(targetLevel);

            List<List<LINE>> Result = new List<List<LINE>>();
            foreach (List<LINE> item in NewBeamGroup)
            {
                Result.Add(TakeOffColumnEdge(columns, item));
            }

            Result = ConnectedEdgeFromMiddleColumns(Result);
            SaveTmp(Result);
            return Result;
        }

        private List<List<LINE>> ClassBeamsByEvelution(List<LINE> BEAMS)
        {
            int[] IsDone = new int[BEAMS.Count];
            List<List<LINE>> BEAMS_Class = new List<List<LINE>>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                List<LINE> tmp = new List<LINE>();
                tmp.Add(BEAMS[i]);
                for (int j = 0; j < BEAMS.Count; j++)
                {
                    if (i != j && IsDone[j] == 0 && Math.Round(BEAMS[i].GetStartPoint().Z,3) == Math.Round(BEAMS[j].GetStartPoint().Z,3))
                    {
                        tmp.Add(BEAMS[j]);
                        IsDone[j] = -1;
                    }
                }
                if (tmp.Count > 1)
                {
                    BEAMS_Class.Add(tmp);
                    IsDone[i] = -1;
                }
            }
            return BEAMS_Class;
        }


        private void CreateFloor(Level targetLevel, List<List<LINE>> NewBeamGroup, FloorType floor_type)
        {

            List<CurveArray> floorCurves = new List<CurveArray>();
            foreach (List<LINE> Beams in NewBeamGroup)
            {
                CurveArray curveArray = new CurveArray();
                //floorCurves.Add(curveArray);

                try
                {
                    foreach (LINE beam in Beams)
                    {
                        curveArray.Append(Line.CreateBound(beam.GetStartPoint(), beam.GetEndPoint()));
                    }

                    using (Transaction trans = new Transaction(this.revitDoc))
                    {
                        FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                        FailureHandler failureHandler = new FailureHandler();
                        failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                        failureHandlingOptions.SetClearAfterRollback(false);
                        trans.SetFailureHandlingOptions(failureHandlingOptions);
                        trans.Start("Create Floors");
                        this.revitDoc.Create.NewFloor(curveArray, floor_type, targetLevel, false);
                        trans.Commit();
                    }

                }
                catch (Exception)
                {

                }
            } 
        }



        /// <summary>
        /// 取得所有的梁
        /// </summary>
        /// <returns></returns>
        private List<LINE> GetAllBeams()
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
                    LocationCurve Locurve = beam.Location as LocationCurve;
                    Line line = Locurve.Curve as Line;
                    LINE LINE = new LINE(line.Origin, new XYZ(line.Origin.X + line.Length * line.Direction.X,
                                                              line.Origin.Y + line.Length * line.Direction.Y,
                                                              line.Origin.Z + line.Length * line.Direction.Z));


                    ElementType type = revitDoc.GetElement(beam.GetTypeId()) as ElementType;
                    Parameter mLevel = beam.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
                    string levelName = mLevel.AsValueString();
                    LINE.Name = beam.Name;
                    LINE.LevelName = levelName;

                    //to get width of section
                    Parameter b = type.LookupParameter("b");
                    LINE.Width = b.AsDouble();

                    ////to get height of section
                    //Parameter h = type.LookupParameter("h");
                    //double height = h.AsDouble(); 

                    if (Math.Round(LINE.GetStartPoint().Z,3)  == Math.Round(LINE.GetEndPoint().Z,3))
                    {
                        if (LINE.GetDirection().X == 1 || LINE.GetDirection().Y == 1 || LINE.GetDirection().X == -1 || LINE.GetDirection().Y == -1)
                        {
                            Beams.Add(LINE);
                        }
                    }

                }
                catch (Exception)
                {

                }

            }
            return Beams;
        }

        /// <summary>
        /// 取得所有梁並將轉換至各樓板邊緣線
        /// </summary>
        /// <param name="BEAMS"></param>
        /// <returns></returns>
        private List<List<LINE>> GetBeamGroup(List<LINE> BEAMS)
        {
            double SHIFTDIST = 2000 / 304.8;

            int number = 0;
            int[] count = new int[BEAMS.Count];
            Dictionary<string, List<int>> pickNumbers = new Dictionary<string, List<int>>();
            Dictionary<string, List<LINE>> DictRes = new Dictionary<string, List<LINE>>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                List<LINE> ResTmp = new List<LINE>();
                List<int> tmpcheckPicked = new List<int>() { };
                for (int j = 0; j < BEAMS.Count; j++)
                {
                    if (j == i) continue;
                    if (CMPPoints(BEAMS[i].GetEndPoint(), BEAMS[j].GetStartPoint(), SHIFTDIST))
                    {
                        ResTmp.Add(BEAMS[j]);
                        tmpcheckPicked.Add(j);
                    }
                    else if (CMPPoints(BEAMS[i].GetEndPoint(), BEAMS[j].GetEndPoint(), SHIFTDIST))
                    {

                        ResTmp.Add(new LINE(BEAMS[j].GetEndPoint(),
                                            BEAMS[j].GetStartPoint(),
                                            BEAMS[j].Name,
                                            BEAMS[j].LevelName,
                                            BEAMS[j].Width));
                        tmpcheckPicked.Add(j);
                    }
                }

                List<LINE> Res = null;
                for (int pp = 0; pp < ResTmp.Count; pp++)
                {
                    bool open = true;
                    Res = new List<LINE> { BEAMS[i], ResTmp[pp] };
                    List<int> checkPicked = new List<int>() { i, tmpcheckPicked[pp] };

                    while (open)
                    {
                        int j = 0;
                        List<LINE> tmpRes = new List<LINE>();
                        tmpRes = Res.ToList();
                        List<int> tmpCheckPicked = new List<int>();
                        while (j < BEAMS.Count)
                        {
                            if (!checkPicked.Contains(j))
                            {
                                if (CMPPoints(Res[Res.Count - 1].GetEndPoint(), BEAMS[j].GetStartPoint(), SHIFTDIST))
                                {
                                    tmpRes.Add(BEAMS[j]);
                                    tmpCheckPicked.Add(j);
                                }
                                else if (CMPPoints(Res[Res.Count - 1].GetEndPoint(), BEAMS[j].GetEndPoint(), SHIFTDIST))
                                {
                                    tmpRes.Add(new LINE(BEAMS[j].GetEndPoint(),
                                                        BEAMS[j].GetStartPoint(),
                                                        BEAMS[j].Name,
                                                        BEAMS[j].LevelName,
                                                        BEAMS[j].Width));
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
                                if (!IsSamePoint(tmp.GetStartPoint(), tmpRes[ii].GetEndPoint()))
                                {
                                    diff.Add(new LINE(tmp.GetStartPoint(), tmpRes[ii].GetEndPoint()).GetLength());
                                }
                                else
                                {
                                    diff.Add(0);
                                }
                            }
                            int po = diff.FindIndex(item => item.Equals(diff.Min()));
                            checkPicked.Add(tmpCheckPicked[po]);
                            Res.Add(tmpRes[Res.Count + po]);
                            tmpCheckPicked.Clear();
                            if (diff.Min() < SHIFTDIST)
                            {
                                open = false;
                            }
                            else if (CMPPoints(Res[0].GetEndPoint(), Res[Res.Count - 1].GetEndPoint(), SHIFTDIST))
                            {
                                open = false;
                                SaveTmp(Res);
                                Res.RemoveAt(0);
                                checkPicked.RemoveAt(0);
                            }
                        }
                        else
                        {
                            open = false;
                        }
                    }

                    pickNumbers[number.ToString()] = checkPicked;
                    DictRes[number.ToString()] = Res;
                    number++;
                }


            }


            ////// Filter Groups
            List<int> sumOf = new List<int>();
            List<string> keys = pickNumbers.Keys.ToList();
            foreach (KeyValuePair<string, List<int>> item in pickNumbers)
            {
                sumOf.Add(item.Value.Sum());
            }

            int[] flag = new int[sumOf.Count];
            for (int ii = 0; ii < sumOf.Count; ii++)
            {
                if (flag[ii] == -1) continue;
                for (int j = 0; j < sumOf.Count; j++)
                {
                    List<LINE> tmpBeams = DictRes[keys[j]];
                    double Len = IsSamePoint(tmpBeams[0].GetStartPoint(), tmpBeams[tmpBeams.Count - 1].GetEndPoint()) ?
                                 0 : (new LINE(tmpBeams[0].GetStartPoint(), tmpBeams[tmpBeams.Count - 1].GetEndPoint())).GetLength();

                    if (ii != j && sumOf[ii] == sumOf[j])
                    {
                        flag[j] = -1;
                    }
                    else if (Len > SHIFTDIST)
                    {
                        flag[j] = -1;
                    }


                }
            }
            List<int> Flag = flag.ToList();
            int[] lastPo = FindAllIndexof(Flag, 0);

            Dictionary<string, List<LINE>> DictResult = new Dictionary<string, List<LINE>>();
            List<List<LINE>> RESULT = new List<List<LINE>>();
            Dictionary<int, List<int>> poo = new Dictionary<int, List<int>>();
            foreach (int pp in lastPo)
            {
                DictResult[keys[pp]] = DictRes[keys[pp]];
                RESULT.Add(DictRes[keys[pp]]);
                poo[pp] = pickNumbers[pp.ToString()];
            }
            return RESULT;
        }

        /// <summary>
        /// 篩選指定樓層的梁
        /// </summary>
        /// <param name="targetLevel_"></param>
        /// <returns></returns>
        private List<LINE> GetTargetFloorBeams(Level targetLevel_)
        {
            double targetLevel = targetLevel_.Elevation;
            List<LINE> BEAMS = GetAllBeams();

            var beams = from bb in BEAMS
                        where bb.LevelName == targetLevel_.Name
                        select bb;

            return beams.ToList();
        }




        /// <summary>
        /// 將樓板邊緣線偏移並連接
        /// </summary>
        /// <param name="BeamGroup"></param>
        /// <returns></returns>
        private List<List<LINE>> BeamConnectAndShiftProcessing(List<List<LINE>> BeamGroup)
        {
            List<List<LINE>> NewBeamGroup = new List<List<LINE>>();
            foreach (List<LINE> Beams in BeamGroup)
            {
                if (Beams.Count < 4) continue;

                double sumOfX = Beams.Sum(item => item.GetStartPoint().X);
                double sumOfY = Beams.Sum(item => item.GetStartPoint().Y);
                XYZ tarPoint = new XYZ(sumOfX / Beams.Count, sumOfY / Beams.Count, 0);
                List<LINE> tmpBeams = new List<LINE>();
                foreach (LINE beam in Beams)
                {
                    double width = beam.Width / 2;
                    tmpBeams.Add(beam.GetShiftLines(tarPoint, width, "IN")[0]);
                }
                NewBeamGroup.Add(tmpBeams);
            }

            ConnectedEdge(ref NewBeamGroup);

            return NewBeamGroup;
        }






        
        /// <summary>
        /// 判斷點是否在封閉曲線內
        /// </summary>
        /// <param name="point"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        private bool IsInner(XYZ point, List<LINE> boundary)
        {
            double minX = boundary.Min(m => m.GetStartPoint().X);
            double maxX = boundary.Max(m => m.GetStartPoint().X);
            double minY = boundary.Min(m => m.GetStartPoint().Y);
            double maxY = boundary.Max(m => m.GetStartPoint().Y);

            if (point.X > minX && point.X < maxX && point.Y > minY && point.Y < maxY)
            {
                return true;
            }
            return false;

        }

        private bool IsSamePoint(XYZ point1, XYZ point2)
        {
            if (point1.X == point2.X && point1.Y == point2.Y && point1.Z == point2.Z)
            {
                return true;
            }
            return false;

        }







        private void ConnectedEdge(ref List<List<LINE>> NewBeamGroup)
        {
            foreach (List<LINE> Beams in NewBeamGroup)
            {
                Dictionary<int, LINE> tmpAddLines = new Dictionary<int, LINE>();
                for (int i = 0; i < Beams.Count; i++)
                {
                    LINE L1 = Beams[i];
                    LINE L2 = i + 1 == Beams.Count ? Beams[0] : Beams[i + 1];
                    if (!L1.IsSameDirection(L2.GetDirection(),true))
                    {
                        XYZ crossPoint = L1.GetCrossPoint(L2);
                        Beams[i].ResetParameters(crossPoint, "EndPoint");
                        if (i + 1 == Beams.Count)
                        {
                            Beams[0].ResetParameters(crossPoint, "StartPoint");
                        }
                        else
                        {
                            Beams[i + 1].ResetParameters(crossPoint, "StartPoint");
                        }
                    } 
                }
            }
        }



        /// <summary>
        /// 取得所有的Column
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<LINE>> GetAllColumnsBoundary(Level targetLevel)
        {
            FilteredElementCollector collector = new FilteredElementCollector(this.revitDoc);
            collector.OfCategory(BuiltInCategory.OST_Columns);
            collector.OfClass(typeof(FamilyInstance));

            IList<Element> columns = collector.ToElements();

            Dictionary<string, List<LINE>> ResultColumns = new Dictionary<string, List<LINE>>();
            int kk = 0;
            foreach (FamilyInstance familyOfColumn in columns)
            {
                Level cLevel = this.revitDoc.GetElement(familyOfColumn.LevelId) as Level;
                if (cLevel.Name == targetLevel.Name)
                {
                    CurveLoop curve = GetFaceEdgelines(familyOfColumn);
                    List<LINE> Curves = new List<LINE>();
                    string Type = "";
                    foreach (Curve cc in curve)
                    {
                        if (cc.GetType().Name == "Line")
                        {
                            Type = "Line";
                            Line Line = cc as Line;
                            Curves.Add(new LINE(Line.Origin, Line.Direction, Line.Length));
                        }
                    }
                    ResultColumns[kk.ToString() + " " + Type] = Curves;
                    kk++;
                }
            }

            return ResultColumns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="NewBeamGroup"></param>
        /// <returns></returns>
        private List<List<LINE>> ConnectedEdgeFromMiddleColumns(List<List<LINE>> NewBeamGroup)
        {
            List<List<LINE>> newFloorGroup = new List<List<LINE>>();
            foreach (List<LINE> Beams in NewBeamGroup)
            {
                Dictionary<int, LINE> tmpAddLines = new Dictionary<int, LINE>();
                for (int i = 0; i < Beams.Count; i++)
                {
                    LINE L1 = Beams[i];
                    LINE L2 = i + 1 == Beams.Count ? Beams[0] : Beams[i + 1];
                    if (L1.IsSameDirection(L2.GetDirection(), true) && !L1.IsPointInLine(L2.GetStartPoint()))
                    {
                        tmpAddLines[i] = new LINE(L1.GetEndPoint(), L2.GetStartPoint());
                    }
                }

                foreach (int ii in tmpAddLines.Keys)
                {
                    Beams.Add(tmpAddLines[ii]);
                }

                int kk = 0;
                List<LINE> newBeams = new List<LINE>();
                int[] flag = new int[Beams.Count];
                flag[kk] = -1;
                newBeams.Add(Beams[0]);
                while (kk < Beams.Count)
                {
                    if (flag[kk] != -1 && newBeams[newBeams.Count - 1].IsPointInLine(Beams[kk].GetStartPoint()))
                    {
                        flag[kk] = -1;
                        newBeams.Add(Beams[kk]);
                        kk = 0;
                    }
                    kk = kk + 1;
                }

                newFloorGroup.Add(newBeams);
                //SaveTmp(newBeams);

            }

            return newFloorGroup;

        }

        private List<LINE> TakeOffColumnEdge(Dictionary<string, List<LINE>> columns, List<LINE> floor)
        {
            foreach (KeyValuePair<string, List<LINE>> column in columns)
            {
                List<LINE> newFloor = new List<LINE>();
                Dictionary<int, List<LINE>> tmpData = new Dictionary<int, List<LINE>>();
                for (int ii = 0; ii < column.Value.Count; ii++)
                {
                    LINE columnEdge = column.Value[ii];
                    for (int jj = 0; jj < floor.Count; jj++)
                    {
                        LINE floorEdge = floor[jj];
                        if (floorEdge.GetSlope() == columnEdge.GetSlope()) continue;
                        XYZ crossPoint = floorEdge.GetCrossPoint(columnEdge);

                        if (floorEdge.IsPointInLine(crossPoint) && columnEdge.IsPointInLine(crossPoint))
                        {
                            XYZ innerPoint = IsInner(columnEdge.GetStartPoint(), floor) ?
                                             columnEdge.GetStartPoint() : columnEdge.GetEndPoint();

                            double dis1 = (crossPoint - floorEdge.GetStartPoint()).GetLength();
                            double dis2 = (crossPoint - floorEdge.GetEndPoint()).GetLength();
                            LINE newLine = null;
                            List<LINE> newList = new List<LINE>();
                            if (dis1 > dis2)
                            {
                                floorEdge.ResetParameters(crossPoint, "EndPoint");
                                newLine = new LINE(crossPoint, innerPoint);
                                newList.Add(floorEdge);
                                newList.Add(newLine);
                            }
                            else
                            {
                                newLine = new LINE(innerPoint, crossPoint);
                                floorEdge.ResetParameters(crossPoint, "StartPoint");
                                newList.Add(newLine);
                                newList.Add(floorEdge);
                            }
                            tmpData[jj] = newList;
                        }
                    }

                }


                for (int kk = 0; kk < floor.Count; kk++)
                {
                    if (tmpData.ContainsKey(kk))
                    {
                        foreach (LINE item in tmpData[kk])
                        {
                            newFloor.Add(item);
                        }
                    }
                    else
                    {
                        newFloor.Add(floor[kk]);
                    }
                }
                floor = new List<LINE>();
                floor = newFloor;
            }

            return floor;
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
            try
            {
                return (new LINE(point1, point2)).GetLength() <= diff ? true : false;

            }
            catch (Exception)
            {

                return true;
            }

        }


        /// <summary>
        /// 幾何 -> 固體 -> 表面 -> 邊
        /// Geomerty -> Solid -> surface -> edges
        /// </summary>
        /// <param name="famulyOfColumns"></param>
        private CurveLoop GetFaceEdgelines(FamilyInstance famulyOfColumns)
        {
            CurveLoop curveLoop = new CurveLoop();
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = famulyOfColumns.get_Geometry(opt);
            if (geomElem != null)
            {
                /// GeometryElement 包含三個枚舉(GeometryInstance, Solid, Solid)
                foreach (var item in geomElem)
                {
                    /// 取得Type為Solid的枚舉
                    if (item.GetType().Name == "Solid")
                    {
                        /// 轉換至Solid型態
                        Solid solid = item as Solid;

                        /// 若無面，略過
                        if (solid.Faces.Size == 0) continue;

                        /// 列舉Solid所有面
                        foreach (Face face in solid.Faces)
                        {
                            /// 轉換至Surface型態
                            Surface surface = face.GetSurface();

                            /// 若為平面 else 圓柱面
                            if (face.GetType().Name == "PlanarFace")
                            {
                                Plane plane = surface as Plane;
                                /// 取得基準面
                                if (plane.Normal.X == 0 && plane.Normal.Y == 0 && plane.Normal.Z == -1)
                                {
                                    curveLoop = face.GetEdgesAsCurveLoops()[0];
                                }
                            }
                            else if (face.GetType().Name == "CylindricalFace")
                            {
                                CylindricalSurface csf = surface as CylindricalSurface;
                            }
                        }
                    }
                }
            }

            return curveLoop;
        }





        private void SaveTmp(List<LINE> data1)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string subFolderPath = Path.Combine(path, "revitTest", "0_SubFolder");
            StreamWriter sw = new StreamWriter(Path.Combine(subFolderPath, "points" + System.IO.Directory.GetFiles(subFolderPath).Count().ToString() + ".txt"));
            foreach (var dd in data1)
            {
                sw.WriteLine(dd.GetStartPoint().X + " " + dd.GetStartPoint().Y + " " + dd.GetStartPoint().Z + " " + dd.GetEndPoint().X + " " + dd.GetEndPoint().Y + " " + dd.GetEndPoint().Z);
                sw.Flush();
            }
            sw.Close();

        }

        private void SaveTmp(List<List<LINE>> data1)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string subFolderPath = Path.Combine(path, "revitTest", "0_SubFolder");
            foreach (string fname in System.IO.Directory.GetFiles(subFolderPath))
            {
                System.IO.File.Delete(fname);
            }
            Directory.CreateDirectory(subFolderPath);
            int kk = 0;
            foreach (List<LINE> tmp in data1)
            {
                StreamWriter sw = new StreamWriter(Path.Combine(subFolderPath, "points" + kk.ToString() + ".txt"));
                foreach (LINE dd in tmp)
                {
                    sw.WriteLine(dd.GetStartPoint().X + " " + dd.GetStartPoint().Y + " " + dd.GetEndPoint().X + " " + dd.GetEndPoint().Y);
                    sw.Flush();
                }
                sw.Close();
                kk++;
            }

        }

        private List<List<LINE>> Will_Do_GetBeamGroup_General(Level targetLevel)
        {
            double SHIFTDIST = 2000 / 304.8;
            List<LINE> BEAMS = GetTargetFloorBeams(targetLevel);

            Dictionary<string, List<int>> pickNumbers = new Dictionary<string, List<int>>();
            Dictionary<string, List<LINE>> DictRes = new Dictionary<string, List<LINE>>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                List<List<LINE>> AllRes = new List<List<LINE>>();
                List<List<int>> AllPicked = new List<List<int>>();
                List<LINE> ResTmp = new List<LINE>() { };
                List<int> tmpcheckPicked = new List<int>() { };
                bool open = true;
                tmpcheckPicked = Will_Do_GetTreeStructure(BEAMS, BEAMS[i], ref ResTmp, tmpcheckPicked, SHIFTDIST);

            }

            return new List<List<LINE>>();
        }

        private List<int> Will_Do_GetTreeStructure(List<LINE> BEAMS, LINE TargetDATA, ref List<LINE> ResTmp, List<int> tmpcheckPicked, double SHIFTDIST)
        {

            for (int j = 0; j < BEAMS.Count; j++)
            {
                if (tmpcheckPicked.Contains(j)) continue;
                bool open = false;
                if (TargetDATA.IsPointInLine(BEAMS[j].GetStartPoint()) ||
                    CMPPoints(TargetDATA.GetEndPoint(), BEAMS[j].GetStartPoint(), SHIFTDIST))
                {
                    open = true;
                    ResTmp.Add(BEAMS[j]);
                    tmpcheckPicked.Add(j);
                }
                else if (TargetDATA.IsPointInLine(BEAMS[j].GetEndPoint()) ||
                         CMPPoints(TargetDATA.GetEndPoint(), BEAMS[j].GetEndPoint(), SHIFTDIST))
                {
                    open = true;
                    ResTmp.Add(new LINE(BEAMS[j].GetEndPoint(), BEAMS[j].GetStartPoint(), BEAMS[j].Name));
                    tmpcheckPicked.Add(j);
                }


            }

            return tmpcheckPicked;
        }
    }
}
