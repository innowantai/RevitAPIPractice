using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace _6_ReadDWG
{
    public class Flatten
    {
        private string fileName;
        string[] Block = new string[] { "", " ", "  ", "   ", "    ", "     ", "      ", "       ", "        ", "         ", "          ", "           ", "            ", "             ", "              " };

        public Flatten()
        {

        }


        public void Merging(VECTOR[] Data1, VECTOR[] Data2)
        {
            VECTOR p1 = Data1[1];
            VECTOR p2 = Data1[3];
            VECTOR p3 = Data2[0];
            VECTOR p4 = Data2[2];

            VECTOR d1 = (p4 - p3).UnitForm();
            VECTOR d2 = (p2 - p1).UnitForm();
            MATRIX RotaMatri_1 = new MATRIX(new double[,] { { d2.X, d2.Y }, { -d2.Y, d2.X } });
            MATRIX RotaMatri_2 = new MATRIX(new double[,] { { d1.X, d1.Y }, { -d2.Y, d2.X } });
            //// 以基準點平移
            Data1 = ShiftFromTargetPoint(Data1, Data1[1]);
            Data2 = ShiftFromTargetPoint(Data2, Data2[0]);
            ShowData(Data1);
            ShowData(Data2);



        }



        private VECTOR[] Rotation(VECTOR[] Data1, MATRIX RotaMatri_1)
        {

            //// 以基準點旋轉
            for (int i = 0; i < Data1.Count(); i++)
            {
                MATRIX vect = new MATRIX(new double[,] { { Data1[i].X }, { Data1[i].Y } });
                MATRIX res = RotaMatri_1.CrossMatrix(vect);
                Data1[i] = new VECTOR(res.Matrix[0, 0], res.Matrix[1, 0], 0);

            }

            return Data1;
        }



        /// <summary>
        /// 將點資料攤平
        /// </summary>
        /// <param name="POINTs"></param>
        /// <returns></returns>
        public VECTOR[] Flattening(VECTOR[] POINTs)
        {
            int TARGET = 0;
            for (int i = 1; i <= POINTs.Length - 3; i++)
            {
                VECTOR Vec_X = null;
                VECTOR Vec_Y = null;
                if (i % 2 == 0)
                {
                    TARGET = i + 1;
                    Vec_X = (POINTs[TARGET - 1] - POINTs[TARGET]).UnitForm();
                    Vec_Y = (POINTs[TARGET + 2] - POINTs[TARGET]).UnitForm();
                }
                else
                {
                    TARGET = i;
                    Vec_X = (POINTs[TARGET + 1] - POINTs[TARGET]).UnitForm();
                    Vec_Y = (POINTs[TARGET + 2] - POINTs[TARGET]).UnitForm();
                }

                VECTOR Cross_Z = Vec_X.Cross(Vec_Y).UnitForm();
                VECTOR ProjectionDir = Cross_Z.Cross(Vec_X);
                POINTs = ShiftFromTargetPoint(POINTs, POINTs[TARGET]);
                POINTs = ProjectionForAll(POINTs, Vec_X, ProjectionDir, Cross_Z);

                for (int j = 0; j < i; j++)
                {
                    VECTOR POINT = POINTs[j];
                    VECTOR newPoint = new VECTOR(POINT.X, Math.Sign(POINT.Y) * Math.Sqrt(POINT.Y * POINT.Y + POINT.Z * POINT.Z), 0);
                    POINTs[j] = newPoint;
                }
            }

            return POINTs;
        }

        /// <summary>
        /// 全部資料分別對X,Y,Z投影
        /// </summary>
        /// <param name="POINTs"></param>
        /// <param name="Vec_X"></param>
        /// <param name="Vec_Y"></param>
        /// <param name="Vec_Z"></param>
        /// <returns></returns>
        private VECTOR[] ProjectionForAll(VECTOR[] POINTs, VECTOR Vec_X, VECTOR Vec_Y, VECTOR Vec_Z)
        {
            for (int j = 0; j < POINTs.Count(); j++)
            {
                double Proj_X = POINTs[j].Dot(Vec_X);
                double Proj_Y = POINTs[j].Dot(Vec_Y);
                double Proj_Z = POINTs[j].Dot(Vec_Z);
                VECTOR newPoint = new VECTOR(Proj_X, Proj_Y, Proj_Z);
                POINTs[j] = newPoint;
            }
            return POINTs;
        }

        /// <summary>
        /// 全部資料以點shiftPoint基準偏移
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="shiftPoint"></param>
        /// <returns></returns>
        private VECTOR[] ShiftFromTargetPoint(VECTOR[] Data, VECTOR shiftPoint)
        {
            List<VECTOR> RESULT = new List<VECTOR>();
            foreach (VECTOR dd in Data)
            {
                RESULT.Add(new VECTOR(dd.X - shiftPoint.X, dd.Y - shiftPoint.Y, dd.Z - shiftPoint.Z));
            }
            return RESULT.ToArray();
        }

        /// <summary>
        /// 顯示點資料
        /// </summary>
        /// <param name="Data"></param>
        public void ShowData(VECTOR[] Data)
        {
            string res = "";
            for (int i = 0; i < Data.Count(); i += 2)
            {
                VECTOR p1 = Data[i];
                VECTOR p2 = Data[i + 1];
                res += GetShowString(p1) + "  " + GetShowString(p2) + "\r\n";
            }
            Console.WriteLine(res);
        }

        private string GetShowString(VECTOR p1)
        {
            int Len = 10;
            int ShowNum = 5;
            string st1 = Math.Round(p1.X, ShowNum).ToString("F4");
            string st2 = Math.Round(p1.Y, ShowNum).ToString("F4");
            string st3 = Math.Round(p1.Z, ShowNum).ToString("F4");
            return Block[Len - st1.Length] + st1 + "," +
                   Block[Len - st2.Length] + st2 + "," +
                   Block[Len - st3.Length] + st3;
        }

    }











}
