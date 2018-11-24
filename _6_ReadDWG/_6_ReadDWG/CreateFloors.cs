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


        public void CreateFloor(Level targetLevel)
        {

            List<List<LINE>> NewBeamGroup = BeamPreProcessing(targetLevel);
            List<CurveArray> floorCurves = new List<CurveArray>();
            foreach (List<LINE> Beams in NewBeamGroup)
            {
                CurveArray curveArray = new CurveArray();
                foreach (LINE beam in Beams)
                {
                    curveArray.Append(Line.CreateBound(beam.GetStartPoint(), beam.GetEndPoint()));
                }
                floorCurves.Add(curveArray);
            }

            FilteredElementCollector collector = new FilteredElementCollector(this.revitDoc);
            collector.OfCategory(BuiltInCategory.OST_Floors);
            var floorTypes = collector.ToList();


            foreach (CurveArray curveArray in floorCurves)
            {

                try
                {
                    using (Transaction trans = new Transaction(this.revitDoc))
                    {
                        //FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                        //FailureHandler failureHandler = new FailureHandler();
                        //failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                        //failureHandlingOptions.SetClearAfterRollback(false);
                        //trans.SetFailureHandlingOptions(failureHandlingOptions);
                        trans.Start("Create Floors");
                        this.revitDoc.Create.NewFloor(curveArray, floorTypes[0] as FloorType, targetLevel, false);
                        trans.Commit();
                    }

                }
                catch (Exception)
                {

                }

            }


        }

        private List<List<LINE>> BeamPreProcessing(Level targetLevel)
        {
            /// 取得所有的梁並轉換至各樓板邊緣線
            List<List<LINE>> BeamGroup = GetBeamGroup(targetLevel);

            SaveTmp(BeamGroup);

            /// 將樓板邊緣線偏移並連接
            List<List<LINE>> NewBeamGroup = BeamConnectAndShiftProcessing(BeamGroup);


            Dictionary<string, List<LINE>> columns = GetAllColumnsBoundary(targetLevel);
            List<List<LINE>> Result = new List<List<LINE>>();
            foreach (List<LINE> item in NewBeamGroup)
            {
                Result.Add(TakeOffColumnEdge(columns, item));
            }

            Result = ConnectedEdgeFromMiddleColumns(Result);
            //SaveTmp(Result);
            return Result;
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


        /// <summary>
        /// Get All Beams
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
                double sumOfX = Beams.Sum(item => item.GetStartPoint().X);
                double sumOfY = Beams.Sum(item => item.GetStartPoint().Y);
                XYZ tarPoint = new XYZ(sumOfX / Beams.Count, sumOfY / Beams.Count, 0);
                List<LINE> tmpBeams = new List<LINE>();
                foreach (LINE beam in Beams)
                {
                    double width = Convert.ToDouble(beam.Name.Split('x')[0].Trim()) / 304.8 / 2;
                    tmpBeams.Add(beam.GetShiftLines(tarPoint, width, "IN")[0]);
                }
                NewBeamGroup.Add(tmpBeams);
            }

            ConnectedEdge(ref NewBeamGroup);

            return NewBeamGroup;
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


        private void SaveTmp(List<LINE> data1)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string subFolderPath = Path.Combine(path, "revitTest", "0_SubFolder");
            StreamWriter sw = new StreamWriter(Path.Combine(subFolderPath, "points" + System.IO.Directory.GetFiles(subFolderPath).Count().ToString() + ".txt"));
            foreach (var dd in data1)
            {
                sw.WriteLine(dd.GetStartPoint().X + " " + dd.GetStartPoint().Y + " " + dd.GetEndPoint().X + " " + dd.GetEndPoint().Y);
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



        private List<int> GetTreeStructure(List<LINE> BEAMS, LINE TargetDATA, ref List<LINE> ResTmp, List<int> tmpcheckPicked, double SHIFTDIST)
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


        private List<List<LINE>> GetBeamGroup(Level targetLevel)
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
                tmpcheckPicked = GetTreeStructure(BEAMS, BEAMS[i], ref ResTmp, tmpcheckPicked, SHIFTDIST); 

            }

            return new List<List<LINE>>();
        }

        /// <summary>
        /// 取得所有梁並將轉換至各樓板邊緣線
        /// </summary>
        /// <param name="targetLevel"></param>
        /// <returns></returns>
        private List<List<LINE>> GetBeamGroup_PAST(Level targetLevel)
        {
            double SHIFTDIST = 2000 / 304.8;
            List<LINE> BEAMS = GetTargetFloorBeams(targetLevel);

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
                        ResTmp.Add(new LINE(BEAMS[j].GetEndPoint(), BEAMS[j].GetStartPoint(), BEAMS[j].Name));
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
                                    tmpRes.Add(new LINE(BEAMS[j].GetEndPoint(), BEAMS[j].GetStartPoint(), BEAMS[j].Name));
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
                                diff.Add(new LINE(tmp.GetStartPoint(), tmpRes[ii].GetEndPoint()).GetLength());
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
                    if (ii != j && sumOf[ii] == sumOf[j])
                    {
                        flag[j] = -1;
                    }
                    else if ((new LINE(tmpBeams[0].GetStartPoint(), tmpBeams[tmpBeams.Count - 1].GetEndPoint())).GetLength() > SHIFTDIST)
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
            List<LINE> BEAMS = GetAllBeams();

            var beams = from bb in BEAMS
                        where bb.GetStartPoint().Z == targetLevel
                        select bb;

            return beams.ToList();
        }

        /// <summary>
        /// Get All Beams
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
}
