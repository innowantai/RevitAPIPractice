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

        public void CreateFloor(Level targetLevel, Level targetLevelCol, FloorType floor_type)
        {
            /// 取得目標樓層所有的梁
            List<LINE> BEAMS = GetTargetFloorBeams(targetLevel);
            List<List<LINE>> Result = this.DataPreProcessing(targetLevel, targetLevelCol, BEAMS);
            this.CreateFloor(targetLevel, Result, floor_type);

        }

        private List<List<LINE>> DataPreProcessing(Level targetLevel, Level targetLevelCol, List<LINE> BEAMs)
        {
            ///// 取得所有梁平均寬度
            double SHIFTs_Width_Of_Beam = 0;
            foreach (LINE item in BEAMs)
            {
                SHIFTs_Width_Of_Beam += item.Width;
            }
            SHIFTs_Width_Of_Beam = SHIFTs_Width_Of_Beam / BEAMs.Count;

            ///// 取得目標樓層所有的柱
            Dictionary<string, List<LINE>> columns = GetAllColumnsBoundary(targetLevelCol);
            int kk = 0;
            double SHIFTs = 0;
            List<XYZ> colCenterPoints = new List<XYZ>();
            foreach (KeyValuePair<string, List<LINE>> item in columns)
            {
                foreach (LINE line in item.Value)
                {
                    SHIFTs += line.GetLength();
                    kk = kk + 1;
                }

                if (item.Value.Count != 0)
                {
                    double All_X = item.Value.Sum(t => t.GetStartPoint().X) / item.Value.Count;
                    double All_Y = item.Value.Sum(t => t.GetStartPoint().Y) / item.Value.Count;
                    colCenterPoints.Add(new XYZ(All_X, All_Y, 0));
                }

            }
            SHIFTs = SHIFTs / kk;


            ////// 延伸梁的邊緣線至柱中心
            /// 找出subBeam,將不做延伸處理
            List<int> NonePo = new List<int>();
            for (int i = 0; i < BEAMs.Count; i++)
            {
                for (int j = 0; j < BEAMs.Count; j++)
                {
                    if (i == j) continue;
                    LINE b1 = BEAMs[i];
                    LINE b2 = BEAMs[j];
                    if (Math.Round(b1.GetSlope(), 3) == Math.Round(b2.GetSlope(), 3)) continue;
                    try
                    { 
                        XYZ crossPoint = b1.GetCrossPoint(b2);
                        if (IsCloseEndsPoint(b1, crossPoint, SHIFTs_Width_Of_Beam) && b2.IsPointInLine(crossPoint))
                        {
                            NonePo.Add(i);
                            break;
                        };
                    }
                    catch (Exception)
                    {
                         
                    }
                }
            }
            List<LINE> Beams_Connect = new List<LINE>();
            for (int i = 0; i < BEAMs.Count; i++)
            {
                if (NonePo.Contains(i))
                {
                    Beams_Connect.Add(BEAMs[i]);
                    continue;
                };

                List<XYZ> col = GetClosedColumn(colCenterPoints, BEAMs[i]);

                XYZ newStartPoint = GetNewEndedPoint(col[0], BEAMs[i].GetStartPoint(), BEAMs[i]);
                XYZ newEndPoint = GetNewEndedPoint(col[1], BEAMs[i].GetEndPoint(), BEAMs[i]);
                if (newStartPoint.X == newEndPoint.X && newStartPoint.Y == newEndPoint.Y)
                {
                    Beams_Connect.Add(BEAMs[i]);
                    continue;
                }
                LINE newBeam = new LINE(newStartPoint, newEndPoint);
                newBeam.Name = BEAMs[i].Name;
                newBeam.Width = BEAMs[i].Width;
                newBeam.LevelName = BEAMs[i].LevelName;
                Beams_Connect.Add(newBeam);



            }



            /// 將所有的梁轉換至各樓板邊緣線
            List<LINE> nonSelected = new List<LINE>();
            List<List<LINE>> BeamGroup = GetBeamGroup(Beams_Connect, ref nonSelected, SHIFTs);

            SaveTmp(BeamGroup);
            /// 將樓板邊緣線偏移並連接
            List<List<LINE>> NewBeamGroup = BeamConnectAndShiftProcessing(BeamGroup);

            SaveTmp(NewBeamGroup);

            if (NewBeamGroup.Count == 0) return NewBeamGroup;

            double newHeight = NewBeamGroup[0][0].GetStartPoint().Z;
            ///// 重設目標樓層所有的柱的高度
            Dictionary<string, List<LINE>> columns2 = new Dictionary<string, List<LINE>>();
            foreach (KeyValuePair<string, List<LINE>> item in columns)
            {

                SaveTmp(item.Value);
                List<LINE> newFloor = new List<LINE>();
                foreach (LINE floor in item.Value)
                {
                    XYZ newSt = new XYZ(floor.GetStartPoint().X,
                                        floor.GetStartPoint().Y,
                                        newHeight);
                    XYZ newEn = new XYZ(floor.GetEndPoint().X,
                                        floor.GetEndPoint().Y,
                                        newHeight);
                    newFloor.Add(new LINE(newSt, newEn));

                }
                columns2[item.Key] = newFloor;
            }

            List<List<LINE>> Result = new List<List<LINE>>();
            foreach (List<LINE> item in NewBeamGroup)
            {
                Result.Add(TakeOffColumnEdge(columns2, item));
            }

            SaveTmp(Result);
            Result = ConnectedEdgeFromMiddleColumns(Result);

            SaveTmp(Result);
            /// 拿掉第一輪subBeam
            List<List<LINE>> Res = TakeOffSubBeams(Result, nonSelected, SHIFTs_Width_Of_Beam);
            List<List<LINE>> Res2 = TakeOffSubBeams(Res, nonSelected, SHIFTs_Width_Of_Beam);
            SaveTmp(Res2);
            return Res2;
        }


        private XYZ GetNewEndedPoint(XYZ ColTarget_, XYZ EndPoint, LINE Beam)
        {
            XYZ ColTarget = new XYZ(ColTarget_.X, ColTarget_.Y, Beam.GetStartPoint().Z);
            XYZ startPoint = Beam.GetStartPoint();
            XYZ oriDir = Beam.GetDirection();
            XYZ newDir = new XYZ(oriDir.Y, -oriDir.X, oriDir.Z);
            LINE VerticalLINE = new LINE(ColTarget, newDir, 1);
            XYZ newPoint = VerticalLINE.GetCrossPoint(Beam);
            double dist = Math.Sqrt((newPoint.X - EndPoint.X) * (newPoint.X - EndPoint.X) +
                          (newPoint.Y - EndPoint.Y) * (newPoint.Y - EndPoint.Y));
            if (dist > Beam.GetLength())
            {
                return EndPoint;
            }
            return VerticalLINE.GetCrossPoint(Beam);

        }

        private List<XYZ> GetClosedColumn(List<XYZ> colCenterPoints, LINE beam)
        {
            List<double> DiffDist1 = new List<double>();
            List<double> DiffDist2 = new List<double>();
            foreach (XYZ item in colCenterPoints)
            {

                XYZ StartPoint = beam.GetStartPoint();
                XYZ EndPoint = beam.GetStartPoint();
                double dist1 = Math.Sqrt((item.X - StartPoint.X) * (item.X - StartPoint.X) +
                                        (item.Y - StartPoint.Y) * (item.Y - StartPoint.Y));
                double dist2 = Math.Sqrt((item.X - EndPoint.X) * (item.X - EndPoint.X) +
                                        (item.Y - EndPoint.Y) * (item.Y - EndPoint.Y));
                DiffDist1.Add(dist1);
                DiffDist2.Add(dist2);
            }

            int p1 = DiffDist1.IndexOf(DiffDist1.Min());
            int p2 = DiffDist2.IndexOf(DiffDist2.Min());
            return new List<XYZ>() { colCenterPoints[p1], colCenterPoints[p2] };


        }



        private List<List<LINE>> TakeOffSubBeams(List<List<LINE>> Result, List<LINE> nonSelected, double SHIFTs)
        {
            SHIFTs = SHIFTs * 2;
            /// 拿掉第一輪subBeam
            List<FloorEdges> result = new List<FloorEdges>();
            foreach (List<LINE> item in Result)
            {
                FloorEdges fe = new FloorEdges(item, nonSelected, SHIFTs);
                List<LINE> subBeam = fe.GetSubBeam();
                result.Add(fe);
            }

            /// 拿掉第二輪subBeam
            List<List<LINE>> Res = new List<List<LINE>>();
            foreach (FloorEdges item in result)
            {
                foreach (List<LINE> floors in item.GetSubFloors())
                {
                    Res.Add(floors);
                }
            }

            return Res;

        }



        /// <summary>
        /// 取得所有的梁(水平與垂直向)
        /// </summary>
        /// <returns></returns>
        private List<LINE> GetAllBeams(Level targetLevel_)
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
                    double width = b.AsDouble();



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


                    if (Math.Round(LINE.GetStartPoint().Z, 3) == Math.Round(LINE.GetEndPoint().Z, 3))
                    {
                        Beams.Add(LINE);
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
            List<LINE> BEAMS = GetAllBeams(targetLevel_);

            var beams = from bb in BEAMS
                        where bb.LevelName == targetLevel_.Name
                        select bb;

            List<LINE> Beams_ = beams.ToList();
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
        private List<List<LINE>> GetBeamGroup(List<LINE> BEAMS, ref List<LINE> nonSelected, double SHIFTDIST)
        {
            //double SHIFTDIST = 2000 / 304.8;

            /// 計算梁的平均長度
            //double SHIFTDIST = BEAMS.Sum(m => m.GetLength()) / BEAMS.Count / 3;

            SHIFTDIST = SHIFTDIST*3 ;

            Dictionary<string, List<int>> pickNumbers = new Dictionary<string, List<int>>();
            Dictionary<string, List<LINE>> DictRes = new Dictionary<string, List<LINE>>();
            for (int i = 0; i < BEAMS.Count; i++)
            {
                int j = 0;
                List<int> picked = new List<int>() { i };
                List<LINE> tmpBeams = new List<LINE>();
                tmpBeams.Add(BEAMS[i]);
                string direction = "";

                while (j < BEAMS.Count)
                {
                    if (tmpBeams.Count > 1 && CMPPoints(tmpBeams[0].GetStartPoint(), tmpBeams[tmpBeams.Count - 1].GetEndPoint(), SHIFTDIST))
                    {
                        break;
                    }

                    LINE target = tmpBeams[tmpBeams.Count - 1];
                    List<int> PickedTmp = new List<int>();
                    List<LINE> ResTmp = GetClosedBeams(BEAMS, target, ref PickedTmp, picked, SHIFTDIST);
                    JudgeDir(tmpBeams, ref direction);
                    List<double> angle = new List<double>();
                    for (int p = 0; p < ResTmp.Count; p++)
                    {
                        XYZ tarDir = target.GetDirection();
                        XYZ itemDir = ResTmp[p].GetDirection();
                        XYZ CrossRes = GetCross(tarDir, itemDir);

                        double DotRes = -tarDir.X * itemDir.X + -tarDir.Y * itemDir.Y;
                        double ang = Math.Acos(Math.Round(DotRes, 2)) * 180 / Math.PI;
                        if (CrossRes.Z > 0)
                        {
                            ang = 360 - ang;
                        }

                        if (direction == "逆")
                        {
                            ang = 360 - ang;
                        }
                        angle.Add(ang);
                    }

                    if (ResTmp.Count != 0)
                    {
                        double minAng = angle.Min();
                        int po1 = FindAllIndexof(angle, minAng)[0];
                        tmpBeams.Add(ResTmp[po1]);
                        picked.Add(PickedTmp[po1]);
                        j = 0;
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

        private string JudgeDir(List<LINE> tmpBeams, ref string direction)
        {
            if (direction != "") return direction;


            for (int i = 0; i < tmpBeams.Count - 1; i++)
            {
                XYZ dir = GetCross(tmpBeams[i].GetDirection(), tmpBeams[i + 1].GetDirection());
                if (Math.Abs(Math.Round(dir.Z * 10)) > 1 && dir.Z > 0)
                {
                    direction = "逆";
                    return direction;
                }
                else if (Math.Abs(Math.Round(dir.Z * 10)) > 1 && dir.Z < 0)
                {
                    direction = "順";
                    return direction;
                }

            }
            return direction;
        }

        private XYZ GetCross(XYZ tarDir, XYZ itemDir)
        {
            return new XYZ(tarDir.Y * itemDir.Z - tarDir.Z * itemDir.Y,
                                 -(tarDir.X * itemDir.Z - tarDir.Z * itemDir.X),
                                   tarDir.X * itemDir.Y - tarDir.Y * itemDir.X);
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

                    if (ii != j && DictRes[keys[ii]].Count == DictRes[keys[j]].Count && sumOf[ii] == sumOf[j])
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


            int kk = 0;
            List<int> takeOff = new List<int>();
            foreach (List<LINE> item in RESULT)
            {
                if (item.Count < 3)
                {
                    foreach (LINE item2 in item)
                    {
                        nonSelected.Add(item2);
                    }
                    takeOff.Add(kk);
                }
                kk++;
            }

            List<List<LINE>> RESULT_f = new List<List<LINE>>();
            for (int i = 0; i < RESULT.Count; i++)
            {
                if (!takeOff.Contains(i))
                {
                    RESULT_f.Add(RESULT[i]);
                }
            }

            return RESULT_f;
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
                //if (Beams.Count < 4) continue;

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
            SaveTmp(NewBeamGroup);
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
                    try
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
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private List<LINE> AdjustCreatedFloorEdge(List<LINE> Beams)
        {
            ///統整同方向
            List<LINE> tmpBeam = new List<LINE>(Beams);
            List<LINE> newBeam = new List<LINE>();
            tmpBeam.Add(tmpBeam[0]);
            List<int> igPo = new List<int>();

            for (int i = 0; i < Beams.Count; i++)
            {
                if (igPo.Contains(i)) continue;

                if (tmpBeam[i].GetSlope() != tmpBeam[i + 1].GetSlope())
                {
                    newBeam.Add(tmpBeam[i]);
                }
                else
                {
                    XYZ p1 = tmpBeam[i].GetStartPoint();
                    XYZ p2 = null;
                    int j = i;
                    while (j < Beams.Count)
                    {
                        if (tmpBeam[j].GetSlope() == tmpBeam[j + 1].GetSlope())
                        {
                            p2 = tmpBeam[j + 1].GetEndPoint();
                            igPo.Add(j + 1);
                        }
                        else
                        {
                            j = Beams.Count;
                        }
                        j++;
                    }
                    newBeam.Add(new LINE(p1, p2));
                }
            }
            return newBeam;
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
                try
                {
                    List<LINE> newBeam_ = AdjustCreatedFloorEdge(Beams);
                List<LINE> newBeam_2 = AdjustCreatedFloorEdge(newBeam_);
                List<LINE> newBeam_3 = new List<LINE>();
                foreach (LINE item in newBeam_2)
                {
                    if (item.GetLength() > 0.01)
                    {
                        newBeam_3.Add(item);
                    }
                }

                List<LINE> newBeam = new List<LINE>();
                newBeam_3.Add(newBeam_3[0]);
                for (int i = 0; i < newBeam_3.Count - 1; i++)
                {
                    if (IsSamePoint(newBeam_3[i].GetStartPoint(), newBeam_3[i + 1].GetStartPoint()) &&
                        IsSamePoint(newBeam_3[i].GetEndPoint(), newBeam_3[i + 1].GetEndPoint()))
                    {

                    }
                    else if (IsSamePoint(newBeam_3[i].GetStartPoint(), newBeam_3[i + 1].GetEndPoint()) &&
                             IsSamePoint(newBeam_3[i].GetEndPoint(), newBeam_3[i + 1].GetStartPoint()))
                    {

                    }
                    else
                    {
                        newBeam.Add(newBeam_3[i]);
                    }
                }


                    // SaveTmp(new List<List<LINE>> { newBeam });

                    CurveArray curveArray = new CurveArray();
                    //floorCurves.Add(curveArray); 
                    foreach (LINE beam in newBeam)
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

            double Len = boundary.Sum(t => t.GetLength());
            LINE shotLine = new LINE(point, new XYZ(0, 1, boundary[0].GetStartPoint().Z), Len);
            int num1 = 0;
            foreach (LINE item in boundary)
            {
                XYZ crossPoint = shotLine.GetCrossPoint(item);
                if (shotLine.IsPointInLine(crossPoint) && item.IsPointInLine(crossPoint))
                {
                    num1++;
                }
            }

            LINE shotLine2 = new LINE(point, new XYZ(0, -1, boundary[0].GetStartPoint().Z), Len);
            int num2 = 0;
            foreach (LINE item in boundary)
            {
                XYZ crossPoint = shotLine2.GetCrossPoint(item);
                if (shotLine2.IsPointInLine(crossPoint) && item.IsPointInLine(crossPoint))
                {
                    num2++;
                }
            }

            if (num1 % 2 == 1 && num2 % 2 == 1) return true;
            if ((num1 + num2) % 2 == 1) return true;
            //double minX = boundary.Min(m => m.GetStartPoint().X);
            //double maxX = boundary.Max(m => m.GetStartPoint().X);
            //double minY = boundary.Min(m => m.GetStartPoint().Y);
            //double maxY = boundary.Max(m => m.GetStartPoint().Y);

            //if (point.X > minX && point.X < maxX && point.Y > minY && point.Y < maxY)
            //{
            //    return true;
            //}
            return false;

        }

        private bool IsSamePoint(XYZ point1, XYZ point2)
        {
            if (Math.Round(point1.X,3) == Math.Round(point2.X,3) && 
                Math.Round(point1.Y,3) == Math.Round(point2.Y,3) && 
                Math.Round(point1.Z,3) == Math.Round(point2.Z,3))
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

            collector.OfCategory(BuiltInCategory.OST_StructuralColumns);
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
                        if (Math.Round(floorEdge.GetSlope(), 3) == Math.Round(columnEdge.GetSlope(), 3)) continue;
                        if (Math.Abs(Math.Round(floorEdge.GetDirection().X, 3)) == Math.Abs(Math.Round(columnEdge.GetDirection().X, 3)) &&
                            Math.Abs(Math.Round(floorEdge.GetDirection().Y, 3)) == Math.Abs(Math.Round(columnEdge.GetDirection().Y, 3))) continue;

                        XYZ crossPoint = floorEdge.GetCrossPoint(columnEdge);


                        try
                        {

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
                                    if (dis2 != 0)
                                    {
                                        newList.Add(newLine);
                                    }
                                }
                                else
                                {
                                    newLine = new LINE(innerPoint, crossPoint);
                                    floorEdge.ResetParameters(crossPoint, "StartPoint");
                                    if (dis1 != 0)
                                    {
                                        newList.Add(newLine);
                                    }
                                    newList.Add(floorEdge);
                                }
                                tmpData[jj] = newList;
                            }
                        }
                        catch (Exception)
                        {

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
                                if (Math.Round(plane.Normal.X, 2) == 0 && Math.Round(plane.Normal.Y, 2) == 0 && Math.Round(plane.Normal.Z, 2) == -1)
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

        private bool IsCloseEndsPoint(LINE line, XYZ crossPoint, double SHIFT)
        {
            double Len1 = Math.Sqrt((line.GetStartPoint().X - crossPoint.X) * (line.GetStartPoint().X - crossPoint.X) +
                                    (line.GetStartPoint().Y - crossPoint.Y) * (line.GetStartPoint().Y - crossPoint.Y));
            double Len2 = Math.Sqrt((line.GetEndPoint().X - crossPoint.X) * (line.GetEndPoint().X - crossPoint.X) +
                                    (line.GetEndPoint().Y - crossPoint.Y) * (line.GetEndPoint().Y - crossPoint.Y));
            if (Len1 < SHIFT || Len2 < SHIFT)
            {
                return true;
            }

            return false;
        }

    }
}
