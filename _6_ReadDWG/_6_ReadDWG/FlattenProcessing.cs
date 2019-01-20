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
using Autodesk.Revit.DB.IFC;
using System.IO;

namespace _6_ReadDWG
{
    public class FlattenProcessing
    {

        private const double TRANS_UNIT = 304.8;
        public static string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        public FlattenProcessing()
        {

        }



        public void ForTest(UIDocument uidoc, Document revitDoc)
        {
            ElementId ele = null;
            Selection selection = uidoc.Selection;
            ICollection<ElementId> element = selection.GetElementIds();
            foreach (ElementId eleID in element)
            {
                ele = eleID;
                break;
            }

            Element e = revitDoc.GetElement(ele);

            Form ff = e as Form;
            var dd = ff.get_CurveLoopReferencesOnProfile(0, 0);
            GeometryElement gg = ff.get_Geometry(new Options());
            List<string> types = new List<string>();
            List<int> CPPoint = new List<int>();
            List<List<XYZ>> SPLINE_POINTs = new List<List<XYZ>>();
            double THICK_LIMIT = 1000 / TRANS_UNIT;
            foreach (Solid item in gg)
            {
                foreach (Face face in item.Faces)
                {
                    HermiteFace HS = face as HermiteFace;
                }


                List<Curve> CURVES = new List<Curve>();
                foreach (Edge edge in item.Edges)
                {
                    Curve cc = edge.AsCurve();
                    HermiteSpline HS = cc as HermiteSpline;
                    //HS.ComputeDerivatives(0, true);
                    //var CP = HS.ControlPoints; 
                    types.Add(cc.GetType().Name);
                    if (HS != null)
                    {
                        CPPoint.Add(HS.ControlPoints.Count);
                        var der1 = HS.ComputeDerivatives(0, true);
                        var der2 = HS.ComputeDerivatives(0.25, true);
                        List<XYZ> tmp = GetSplitedPointsFromEachType(HS, 0);
                        SPLINE_POINTs.Add(tmp);
                        CURVES.Add(cc);
                    }
                    else if (cc.Length > THICK_LIMIT)
                    {

                        CURVES.Add(cc);

                    }
                }
                FilterIndicatedFlattenClosedCurves(CURVES);
                // SaveTest(SPLINE_POINTs);
                //Dictionary<int, FlattenGroup> DATA = GetPairFlattenedData(SPLINE_POINTs);
                //Plattening(DATA);
            }
        }



        private void FilterIndicatedFlattenClosedCurves(List<Curve> CURVES)
        {
            List<LINE> LINE_Group = new List<LINE>();
            foreach (Curve item in CURVES)
            {
                LINE_Group.Add(new LINE(item.GetEndPoint(0), item.GetEndPoint(1)));
            }



        }


        /// <summary>
        /// 將線段進行點分割
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private List<XYZ> GetSplitedPointsFromEachType(Curve curve, int num)
        {
            int kk = 0;
            string CurveType = curve.GetType().Name;
            List<XYZ> tmp = new List<XYZ>();
            int SplitedNum = Convert.ToInt32(curve.Length * TRANS_UNIT / 200);
            double span = num == 16 ? 16 : 1.0 / SplitedNum;

            switch (CurveType)
            {
                case "HermiteSpline":
                    HermiteSpline HS = curve as HermiteSpline;
                    for (double i = 0; i <= 1; i += span)
                    {
                        tmp.Add(HS.ComputeDerivatives(i, true).Origin);
                        kk++;
                    }
                    break;

                case "Line":
                    Line line = curve as Line;
                    double Len = curve.Length / SplitedNum;
                    for (int i = 0; i <= SplitedNum; i++)
                    {
                        XYZ newPoint = new XYZ(line.Origin.X + line.Direction.X * Len * i,
                                               line.Origin.Y + line.Direction.Y * Len * i,
                                               line.Origin.Z + line.Direction.Z * Len * i);
                        tmp.Add(newPoint);
                    }
                    break;

                case "Arc":
                    break;

                default:
                    break;
            }

            return tmp;
        }



        /// <summary>
        /// 展平處理
        /// </summary>
        /// <param name="Pair_Datas"></param>
        private void Plattening(Dictionary<int, FlattenGroup> Pair_Datas)
        {
            string SAVEPATH = Path.Combine(TargetPath, "FlattenResult");
            Directory.CreateDirectory(SAVEPATH);
            foreach (KeyValuePair<int, FlattenGroup> item in Pair_Datas)
            {
                string savePath = Path.Combine(SAVEPATH, "Result_" + item.Key.ToString() + ".csv");
                item.Value.Flattening();
                item.Value.SaveToCSV(savePath);
            }
        }



        private Dictionary<int, FlattenGroup> GetPairFlattenedData(List<List<XYZ>> SPLINE_POINTs)
        {
            /// 將XYZ物件轉換至VECTOR物件
            List<List<VECTOR>> POINTs = new List<List<VECTOR>>();
            foreach (List<XYZ> item in SPLINE_POINTs)
            {
                List<VECTOR> vector = new List<VECTOR>();
                for (int i = 0; i < item.Count; i++)
                {
                    vector.Add(new VECTOR(item[i].X, item[i].Y, item[i].Z));
                }
                POINTs.Add(vector);
            }


            int kk = 0;
            Dictionary<int, FlattenGroup> Pair_POINTs = new Dictionary<int, FlattenGroup>();
            for (int i = 0; i < POINTs.Count; i += 4)
            {
                List<VECTOR> tmp1 = POINTs[i];
                List<VECTOR> tmp2 = POINTs[i + 1];
                List<VECTOR> tmp3 = POINTs[i + 2];
                List<VECTOR> tmp4 = POINTs[i + 3];
                Pair_POINTs[kk] = new FlattenGroup(tmp1, tmp3);
                kk++;
                Pair_POINTs[kk] = new FlattenGroup(tmp2, tmp4);
                kk++;

            }
            return Pair_POINTs;
        }




        private void SaveTest(List<List<XYZ>> SPLINE_POINTs)
        {
            double UNITs = 304.8;
            StreamWriter sw = new StreamWriter(Path.Combine(TargetPath, "Data.csv"));
            foreach (List<XYZ> item in SPLINE_POINTs)
            {
                string res = "";
                foreach (XYZ data in item)
                {
                    res = Math.Round(data.X * UNITs, 8).ToString() + "," + Math.Round(data.Y * UNITs, 8).ToString() + "," + Math.Round(data.Z * UNITs, 8).ToString();
                    sw.WriteLine(res);
                }
                sw.WriteLine("  ");
            }
            sw.Close();

        }


    }





    public class FlattenGroup
    {
        private List<VECTOR> Data1;
        private List<VECTOR> Data2;
        public List<VECTOR> Group_Data_Before;
        public List<VECTOR> Group_Data_After;
        public List<VECTOR> Splited_Data_1_Before;
        public List<VECTOR> Splited_Data_2_Before;
        public List<VECTOR> Splited_Data_1_After;
        public List<VECTOR> Splited_Data_2_After;
        private const double TRANS_UNIT = 304.8;
        public FlattenGroup(List<VECTOR> data1, List<VECTOR> data2)
        {
            data2.Reverse();
            this.Data1 = data1;
            this.Data2 = data2;
            this.Splited_Data_1_Before = this.Data1.ToList();
            this.Splited_Data_2_Before = this.Data2.ToList();
            this.Splited_Data_1_After = new List<VECTOR>();
            this.Splited_Data_2_After = new List<VECTOR>();
        }

        public void Flattening()
        {
            Flatten ff = new Flatten();
            this.Group_Data_Before = this.Combinating();
            this.Group_Data_After = ff.Flattening(this.Group_Data_Before.ToArray()).ToList();
            SplitingRESULT();
        }

        private List<VECTOR> Combinating()
        {

            List<VECTOR> RESULT = new List<VECTOR>();
            for (int i = 0; i < this.Data1.Count; i++)
            {
                RESULT.Add(this.Data1[i]);
                RESULT.Add(this.Data2[i]);
            }
            return RESULT;
        }

        private void SplitingRESULT()
        {
            for (int i = 0; i < this.Group_Data_After.Count; i += 2)
            {
                this.Splited_Data_1_After.Add(this.Group_Data_After[i]);
                this.Splited_Data_2_After.Add(this.Group_Data_After[i + 1]);
            }
        }

        public void SaveToCSV(string savePath)
        {
            using (StreamWriter sw = new StreamWriter(savePath))
            {
                string tmpData = "";
                for (int i = 0; i < this.Splited_Data_1_Before.Count; i++)
                {
                    tmpData = GetString(this.Splited_Data_1_Before[i]) + ",," +
                              GetString(this.Splited_Data_2_Before[i]) + ",,," +
                              GetString(this.Splited_Data_1_After[i]) + ",," +
                              GetString(this.Splited_Data_2_After[i]);
                    sw.WriteLine(tmpData);
                }
            }

        }

        private string GetString(VECTOR p1)
        {
            return (p1.X * TRANS_UNIT).ToString() + "," + (p1.Y * TRANS_UNIT).ToString() + "," + (p1.Z * TRANS_UNIT).ToString();
        }
    }

}
