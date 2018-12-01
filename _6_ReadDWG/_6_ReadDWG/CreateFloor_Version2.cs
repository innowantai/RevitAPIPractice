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
    class CreateFloor_Version2
    {
        private Document revitDoc;

        public CreateFloor_Version2(Document revitDoc_)
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
        }

        private List<List<LINE>> DataPreProcessing(Level targetLevel, List<LINE> BEAMs)
        {
            /// 將所有的梁轉換至各樓板邊緣線
            List<LINE> nonSelected = new List<LINE>();
            List<List<LINE>> BeamGroup = GetBeamGroup(BEAMs, ref nonSelected);

            /// 將樓板邊緣線偏移並連接
            List<List<LINE>> NewBeamGroup = BeamConnectAndShiftProcessing(BeamGroup);

            ///// 取得目標樓層所有的柱
            Dictionary<string, List<LINE>> columns = GetAllColumnsBoundary(targetLevel);

            List<List<LINE>> Result = new List<List<LINE>>();
            foreach (List<LINE> item in NewBeamGroup)
            {
                Result.Add(TakeOffColumnEdge(columns, item));
            }

            Result = ConnectedEdgeFromMiddleColumns(Result);

            int pastNum = Result.Count;
            int[] takeOff = new int[nonSelected.Count];
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < nonSelected.Count; i++)
                {
                    if (takeOff[i] == -1) continue;
                    Result = TakeOffSubBeams(nonSelected[i], Result);
                    if (pastNum != Result.Count)
                    {
                        takeOff[i] = -1;
                        pastNum = Result.Count;
                    }
                }
            }

            return Result;
        }


        private List<List<LINE>> TakeOffSubBeams(LINE target, List<List<LINE>> Result)
        {
            List<int> takeOffRecored = new List<int>();
            List<List<LINE>> newFloors = new List<List<LINE>>();
            for (int i = 0; i < Result.Count; i++)
            {  
                List<LINE> item = Result[i];
                List<XYZ> CrossPoints = new List<XYZ>();
                List<List<LINE>> splitLines = new List<List<LINE>>();
                List<int> picked = new List<int>();
                for (int j = 0; j < item.Count; j++)
                {
                    LINE edgeLine = item[j];
                    if (!target.IsSameDirection(edgeLine.GetDirection(), true))
                    {
                        XYZ crossPoint = target.GetCrossPoint(edgeLine);
                        if (edgeLine.IsPointInLine(crossPoint) && target.IsPointInLine(crossPoint))
                        {
                            CrossPoints.Add(crossPoint);
                            LINE L1 = new LINE(edgeLine.GetStartPoint(), crossPoint);
                            LINE L2 = new LINE(crossPoint, edgeLine.GetEndPoint());
                            splitLines.Add(new List<LINE>() { L1, L2 });
                            picked.Add(j);
                        }
                    }
                }

                if (CrossPoints.Count == 2 && Math.Round(CrossPoints[1].GetLength(), 4) != Math.Round(CrossPoints[0].GetLength(), 4))
                { 
                    takeOffRecored.Add(i);
                    double dd = target.Width / 2;
                    XYZ targetPoint = item[0].GetStartPoint();
                    LINE newLine1 = (new LINE(CrossPoints[0], CrossPoints[1])).GetShiftLines(targetPoint, dd, "IN")[0];
                    LINE newLine2 = (new LINE(CrossPoints[1], CrossPoints[0])).GetShiftLines(targetPoint, dd, "OUT")[0];
                    /// getPart1
                    List<LINE> newFloor1 = new List<LINE>();
                    for (int k = 0; k < item.Count; k++)
                    {
                        if (k == picked[0])
                        {
                            LINE offSetLine = splitLines[0][0];
                            offSetLine.ResetParameters(newLine1.GetStartPoint(), "EndPoint");
                            newFloor1.Add(offSetLine);
                            newFloor1.Add(newLine1);
                        }
                        else if (k == picked[1])
                        {
                            LINE offSetLine = splitLines[1][1];
                            offSetLine.ResetParameters(newLine1.GetEndPoint(), "StartPoint");
                            newFloor1.Add(offSetLine);
                        }
                        else if (k > picked[1] || k < picked[0])
                        {
                            newFloor1.Add(item[k]);
                        }
                    }
                    /// getPart2
                    List<LINE> newFloor2 = new List<LINE>();
                    for (int k = 0; k < item.Count; k++)
                    {
                        if (k == picked[0])
                        {
                            LINE offSetLine = splitLines[0][1];
                            offSetLine.ResetParameters(newLine2.GetEndPoint(), "StartPoint");
                            newFloor2.Add(offSetLine);
                        }
                        else if (k == picked[1])
                        {
                            LINE offSetLine = splitLines[1][0];
                            offSetLine.ResetParameters(newLine2.GetStartPoint(), "EndPoint");
                            newFloor2.Add(offSetLine);
                            newFloor2.Add(newLine2);
                        }
                        else if (k < picked[1] && k > picked[0])
                        {
                            newFloor2.Add(item[k]);
                        }
                    }
                    newFloors.Add(newFloor1);
                    newFloors.Add(newFloor2);
                }
            }

            var nnew = takeOffRecored.OrderByDescending(m => m);
            foreach (var ii in takeOffRecored.OrderByDescending(m => m))
            {
                Result.RemoveAt(ii);
            }

            foreach (List<LINE> item in newFloors)
            {
                Result.Add(item);
            }


            return Result;
        }



        /// <summary>
        /// 取得所有的梁(水平與垂直向)
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

                    if (Math.Round(LINE.GetStartPoint().Z, 3) == Math.Round(LINE.GetEndPoint().Z, 3))
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
        /// 照梁的高度分類
        /// </summary>
        /// <param name="BEAMS"></param>
        /// <returns></returns>
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
                    if (i != j && IsDone[j] == 0 && Math.Round(BEAMS[i].GetStartPoint().Z, 3) == Math.Round(BEAMS[j].GetStartPoint().Z, 3))
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

        /// <summary>
        /// 取得所有梁並將轉換至各樓板邊緣線
        /// </summary>
        /// <param name="BEAMS"></param>
        /// <returns></returns>
        private List<List<LINE>> GetBeamGroup(List<LINE> BEAMS, ref List<LINE> nonSelected)
        {
            double SHIFTDIST = 1000 / 304.8;

            Dictionary<string, List<int>> pickNumbers = new Dictionary<string, List<int>>();
            Dictionary<string, List<LINE>> DictRes = new Dictionary<string, List<LINE>>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                if (BEAMS[i].GetDirection().Y == 1 || BEAMS[i].GetDirection().Y == -1) continue;
                int j = 0;
                List<int> picked = new List<int>() { i };
                List<LINE> tmpBeams = new List<LINE>();
                if (BEAMS[i].GetDirection().X == 1)
                {
                    tmpBeams.Add(BEAMS[i]);
                }
                else
                {
                    tmpBeams.Add(new LINE(BEAMS[i].GetEndPoint(), BEAMS[i].GetStartPoint(), BEAMS[i].Name, BEAMS[i].LevelName, BEAMS[i].Width));
                }

                while (j < BEAMS.Count)
                {
                    LINE target = tmpBeams[tmpBeams.Count - 1];
                    List<int> PickedTmp = new List<int>();
                    List<LINE> ResTmp = GetClosedBeams(BEAMS, target, ref PickedTmp, picked, SHIFTDIST);
                    if (target.GetDirection().X == 1)
                    {
                        LINE res1 = ResTmp.Find(m => m.GetDirection().Y == -1);
                        LINE res = res1 == null ? ResTmp.Find(m => m.GetDirection().X == 1) : res1;
                        if (res != null)
                        {
                            int po1 = FindAllIndexof(ResTmp, res)[0];
                            tmpBeams.Add(ResTmp[po1]);
                            picked.Add(PickedTmp[po1]);
                            j = 0;
                        }
                        else
                        {
                            j++;
                        }
                    }
                    else if (target.GetDirection().Y == -1)
                    {
                        LINE res1 = ResTmp.Find(m => m.GetDirection().X == -1);
                        LINE res = res1 == null ? ResTmp.Find(m => m.GetDirection().Y == -1) : res1;
                        if (res != null)
                        {
                            int po1 = FindAllIndexof(ResTmp, res)[0];
                            tmpBeams.Add(ResTmp[po1]);
                            picked.Add(PickedTmp[po1]);
                            j = 0;
                        }
                        else
                        {
                            j++;
                        }
                    }
                    else if (target.GetDirection().X == -1)
                    {
                        LINE res1 = ResTmp.Find(m => m.GetDirection().Y == 1);
                        LINE res = res1 == null ? ResTmp.Find(m => m.GetDirection().X == -1) : res1;
                        if (res != null)
                        {
                            int po1 = FindAllIndexof(ResTmp, res)[0];
                            tmpBeams.Add(ResTmp[po1]);
                            picked.Add(PickedTmp[po1]);
                            j = 0;
                        }
                        else
                        {
                            j++;
                        }

                    }
                    else if (target.GetDirection().Y == 1)
                    {
                        if (CMPPoints(tmpBeams[0].GetStartPoint(), tmpBeams[tmpBeams.Count - 1].GetEndPoint(), SHIFTDIST))
                        {
                            j = BEAMS.Count + 100;
                        }
                        else
                        {
                            LINE res = ResTmp.Find(m => m.GetDirection().Y == 1);
                            if (res != null)
                            {
                                int po1 = FindAllIndexof(ResTmp, res)[0];
                                tmpBeams.Add(ResTmp[po1]);
                                picked.Add(PickedTmp[po1]);
                            }
                            else
                            {
                                j++;
                            }
                        }
                    }
                    else
                    {
                        j++;
                    }
                }
                DictRes[i.ToString()] = tmpBeams;
                pickNumbers[i.ToString()] = picked;
            }

            return TakeOffSameRegion(ref nonSelected, pickNumbers, DictRes, BEAMS, SHIFTDIST);

        }

        /// <summary>
        /// 拿掉重複的樓板區域
        /// </summary>
        /// <param name="pickNumbers"></param>
        /// <param name="DictRes"></param>
        /// <param name="SHIFTDIST"></param>
        /// <returns></returns>
        private List<List<LINE>> TakeOffSameRegion(ref List<LINE> nonSelected, Dictionary<string, List<int>> pickNumbers, Dictionary<string, List<LINE>> DictRes, List<LINE> BEAMs, double SHIFTDIST)
        {

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
            Dictionary<string, List<int>> poo = new Dictionary<string, List<int>>();
            foreach (int pp in lastPo)
            {
                DictResult[keys[pp]] = DictRes[keys[pp]];
                RESULT.Add(DictRes[keys[pp]]);
                poo[keys[pp]] = pickNumbers[keys[pp]];
            }


            ///Get no-selected beams
            int[] IsPicked = new int[BEAMs.Count];
            foreach (KeyValuePair<string, List<int>> item in poo)
            {
                foreach (int num in item.Value)
                {
                    IsPicked[num] = -1;
                }
            }

            foreach (int item in FindAllIndexof(IsPicked, 0))
            {
                nonSelected.Add(BEAMs[item]);
            }
            return RESULT;
        }


        /// <summary>
        /// 取得與目標梁最靠近的梁群
        /// </summary>
        /// <param name="BEAMS"></param>
        /// <param name="target"></param>
        /// <param name="tmpcheckPicked"></param>
        /// <param name="picked"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private List<LINE> GetClosedBeams(List<LINE> BEAMS, LINE target, ref List<int> tmpcheckPicked, List<int> picked, double SHIFTDIST)
        {
            List<LINE> ResTmp = new List<LINE>();
            for (int j = 0; j < BEAMS.Count; j++)
            {
                if (picked.Contains(j)) continue;

                if (CMPPoints(target.GetEndPoint(), BEAMS[j].GetStartPoint(), SHIFTDIST))
                {
                    ResTmp.Add(BEAMS[j]);
                    tmpcheckPicked.Add(j);
                }
                else if (CMPPoints(target.GetEndPoint(), BEAMS[j].GetEndPoint(), SHIFTDIST))
                {
                    ResTmp.Add(new LINE(BEAMS[j].GetEndPoint(),
                                        BEAMS[j].GetStartPoint(),
                                        BEAMS[j].Name,
                                        BEAMS[j].LevelName,
                                        BEAMS[j].Width));
                    tmpcheckPicked.Add(j);
                }
            }
            return ResTmp;
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
        /// 連接梁接點
        /// </summary>
        /// <param name="NewBeamGroup"></param>
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
        /// 創建樓板
        /// </summary>
        /// <param name="targetLevel"></param>
        /// <param name="NewBeamGroup"></param>
        /// <param name="floor_type"></param>
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
                //SaveTmp(Beams);
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
                SaveTmp(Beams);
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

    }
}
