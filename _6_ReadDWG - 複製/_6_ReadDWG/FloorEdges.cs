using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.DB;

namespace _6_ReadDWG
{
    public class FloorEdges
    {
        private List<LINE> Floor;
        private List<LINE> subBeam;
        private List<LINE> BEAMS;
        private double SHIFT;
        private List<FloorEdges> Floors;

        public FloorEdges(List<LINE> Floor_)
        {
            this.Floor = Floor_;
        }

        public FloorEdges(List<LINE> Floor_, List<LINE> BEAMS_, double SHIFT_)
        {
            this.Floor = Floor_;
            this.BEAMS = BEAMS_;
            this.subBeam = new List<LINE>();

            this.SHIFT = SHIFT_ ;
            SubBeamProcess();
            SplitFloor_Main();
        }

        private void SplitFloor_Main()
        {
            List<LINE> subBeams = new List<LINE>(this.subBeam);
            List<FloorEdges> floor = new List<FloorEdges>() { new FloorEdges(this.Floor) };


            int kk = subBeams.Count;
            while (kk != 0)
            {
                floor = SplitFloor_Sub(floor, ref subBeams);
                kk = kk - 1;
            }
            Floors = floor;

        }
        private List<FloorEdges> SplitFloor_Sub(List<FloorEdges> floor, ref List<LINE> BRAMS)
        {
            Dictionary<int, List<FloorEdges>> res = new Dictionary<int, List<FloorEdges>>();
            int kk = 0;
            List<int> record = new List<int>();
            foreach (FloorEdges item in floor)
            {
                for (int i = 0; i < BRAMS.Count; i++)
                {

                    LINE beams = BRAMS[i];
                    List<FloorEdges> floor_split = SplitFloorBySubBeam(item.GetFloor(), beams);
                    res[kk] = floor_split;
                    if (floor_split.Count > 1)
                    {
                        record.Add(i);
                        break;
                    };
                } 
                kk++;
            }

            if (record.Count == 0)
            {
                return floor;
            }

            List<FloorEdges> newRes = new List<FloorEdges>();
            foreach (KeyValuePair<int, List<FloorEdges>> item in res)
            {
                foreach (FloorEdges floorEdge in item.Value)
                {
                    newRes.Add(floorEdge);
                }
            }

            List<LINE> takeOffSubBeam = new List<LINE>();
            for (int i = 0; i < BRAMS.Count; i++)
            {
                if (!record.Contains(i))
                {
                    takeOffSubBeam.Add(BRAMS[i]);
                }
            }

            BRAMS = takeOffSubBeam;


            return newRes;
        }




        public List<List<LINE>> GetSubFloors()
        {
            List<List<LINE>> Result = new List<List<LINE>>();
            foreach (FloorEdges item in this.Floors)
            {
                Result.Add(item.GetFloor());
            }
            return Result;
        }


        private void SubBeamProcess()
        {
            List<LINE> tmpSubBeam = new List<LINE>();
            foreach (LINE subbeam in this.BEAMS)
            {
                foreach (LINE floorEdge in this.Floor)
                {
                    if (Math.Abs(Math.Round(floorEdge.GetSlope(), 3)) ==
                        Math.Abs(Math.Round(subbeam.GetSlope(), 3))) continue;

                    XYZ crossPoint = floorEdge.GetCrossPoint(subbeam);

                    if (IsCloseEndsPoint(subbeam, crossPoint) && floorEdge.IsPointInLine(crossPoint))
                    {
                        tmpSubBeam.Add(subbeam);
                        break;
                    }
                }
            }

            this.subBeam = tmpSubBeam.OrderByDescending(t => t.GetLength()).ToList();

        }





        private List<FloorEdges> SplitFloorBySubBeam(List<LINE> floor, LINE subbeam)
        {
            List<FloorEdges> Floors = new List<FloorEdges>();
            /////// 檢查subBeam與板邊界是否有兩個交點 
            List<int> CrossPo = new List<int>();
            for (int i = 0; i < floor.Count; i++)
            {
                LINE floorEdge = floor[i];
                if (Math.Abs(Math.Round(floorEdge.GetSlope(), 3)) == Math.Abs(Math.Round(subbeam.GetSlope(), 3))) continue;

                XYZ crossPoint = floorEdge.GetCrossPoint(subbeam);

                if (IsCloseEndsPoint(subbeam, crossPoint) && floorEdge.IsPointInLine(crossPoint))
                {
                    CrossPo.Add(i);
                }

            }

            /////// 沒有兩個交點則不處理直接回傳
            if (CrossPo.Count != 2)
            {
                FloorEdges Floor = new FloorEdges(floor);
                Floors.Add(Floor);
                return Floors;
            }

            /////// 有兩個交點則開始處理梁的分離

            /// 將板轉換成節點
            List<XYZ> nodes = new List<XYZ>();
            foreach (LINE item in floor)
            {
                nodes.Add(item.GetStartPoint());
            }
            nodes.Add(floor[floor.Count - 1].GetEndPoint());

            /// 取的偏移後的交點
            XYZ targetPoint = floor[CrossPo[0]].GetStartPoint();
            LINE offSetLine_1 = subbeam.GetShiftLines(floor[CrossPo[0]].GetStartPoint(), subbeam.Width / 2, "IN")[0];
            LINE offSetLine_2 = subbeam.GetShiftLines(floor[CrossPo[0]].GetEndPoint(), subbeam.Width / 2, "IN")[0];
            XYZ crossPoint1 = offSetLine_1.GetCrossPoint(floor[CrossPo[0]]);
            XYZ crossPoint2 = offSetLine_1.GetCrossPoint(floor[CrossPo[1]]);
            XYZ crossPoint3 = offSetLine_2.GetCrossPoint(floor[CrossPo[0]]);
            XYZ crossPoint4 = offSetLine_2.GetCrossPoint(floor[CrossPo[1]]);
            List<XYZ> crossPoint_1 = new List<XYZ>() { crossPoint1, crossPoint2 };
            List<XYZ> crossPoint_2 = new List<XYZ>() { crossPoint3, crossPoint4 };

            /// Part 1
            int kk = 0;
            List<XYZ> part_1 = new List<XYZ>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (i > CrossPo[0] && i <= CrossPo[1] && kk < 2)
                {
                    part_1.Add(crossPoint_1[kk]);
                    kk = kk + 1;
                }
                else if (i <= CrossPo[0] || i > CrossPo[1])
                {
                    part_1.Add(nodes[i]);
                }
            }

            /// Part 2
            kk = 0;
            List<XYZ> part_2 = new List<XYZ>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (i > CrossPo[0] && i <= CrossPo[1])
                {
                    part_2.Add(nodes[i]);
                }
                else if ((i == CrossPo[0] || i == CrossPo[1] + 1) && kk < 2)
                {
                    part_2.Add(crossPoint_2[kk]);
                    kk = kk + 1;
                }
            }
            part_2.Add(crossPoint_2[0]);

            try
            { 
                /// 將節點轉換成LINE
                List<LINE> newFloor_1 = new List<LINE>();
                for (int i = 0; i < part_1.Count() - 1; i++)
                {
                    newFloor_1.Add(new LINE(part_1[i], part_1[i + 1]));
                }

                List<LINE> newFloor_2 = new List<LINE>();
                for (int i = 0; i < part_2.Count() - 1; i++)
                {
                    newFloor_2.Add(new LINE(part_2[i], part_2[i + 1]));
                }

                Floors.Add(new FloorEdges(newFloor_1));
                Floors.Add(new FloorEdges(newFloor_2));
            }
            catch (Exception)
            {

                return new List<FloorEdges>() { new FloorEdges(floor) };
            }

            return Floors;
        }

        private bool IsCloseEndsPoint(LINE line, XYZ crossPoint)
        {
            double Len1 = Math.Sqrt((line.GetStartPoint().X - crossPoint.X) * (line.GetStartPoint().X - crossPoint.X) +
                                    (line.GetStartPoint().Y - crossPoint.Y) * (line.GetStartPoint().Y - crossPoint.Y));
            double Len2 = Math.Sqrt((line.GetEndPoint().X - crossPoint.X) * (line.GetEndPoint().X - crossPoint.X) +
                                    (line.GetEndPoint().Y - crossPoint.Y) * (line.GetEndPoint().Y - crossPoint.Y));
            if (Len1 < this.SHIFT || Len2 < this.SHIFT)
            {
                return true;
            }

            return false;
        }



        public List<LINE> GetSubBeam()
        {
            return this.subBeam;
        }


        public List<LINE> GetFloor()
        {
            return this.Floor;
        }
    }
}
