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
    public class CreateFloorByCADHashRegion
    {
        const double _eps = 1.0e-9;
        public FindRevitElements RevFind = new FindRevitElements();
        public void Main_Create(Document revitDoc, UIDocument uidoc)
        {
            /// 建立CAD處理物件
            GetCADImformation GetCADImformation = new GetCADImformation(false, false, false);
            GetCADImformation.CADProcessing(uidoc);

            /// 取得CAD Hash區域
            Dictionary<string, List<List<LINE>>> FloorRegions = GetCADImformation.LayersAndClosedRegions;

            /// 若沒選擇CAD 則回傳
            if (FloorRegions.Count == 0) return;

            /// 取得所有樓層
            List<Level> levels = RevFind.GetLevels(revitDoc);
            /// 取得所有板的種類 
            List<Element> floorTypes = RevFind.GetDocFloorTypes(revitDoc);
            /// 建立GUI建面並呼叫
            Form_CreateFloorByCADHash Form = new Form_CreateFloorByCADHash(FloorRegions.Keys.ToList(), levels, floorTypes);
            Form.ShowDialog();


            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                double SHIFT = Convert.ToDouble(Form.txtShift.Text) / 304.8;
                string Layers = Form.cmbCADLayers.SelectedItem.ToString();
                bool IsStructural = Form.chIsStructural.Checked ? true : false;
                Level level = levels[Form.cmbBaseLevels.SelectedIndex];
                FloorType floorType = floorTypes[Form.cmbFloorTypes.SelectedIndex] as FloorType;

                bool IndicatiedLayers = Form.chIsIndicatedLayers.Checked;
                List<List<LINE>> Region = RegionProcess(IndicatiedLayers, Layers, FloorRegions);
                CreateFloor(revitDoc, level, Region, floorType, SHIFT, IsStructural);
            }


        }

        private void CreateFloor(Document revitDoc, Level targetLevel, List<List<LINE>> NewBeamGroup, FloorType floor_type, double SHIFT, bool IsStructural)
        {
             
            List<CurveArray> edges = new List<CurveArray>(); 
            foreach (List<LINE> Beams in NewBeamGroup)
            {
                CurveArray curveArray = new CurveArray(); 
                foreach (LINE beam in Beams)
                {
                    XYZ startPoint = new XYZ(beam.GetStartPoint().X, beam.GetStartPoint().Y, SHIFT);
                    XYZ endPoint = new XYZ(beam.GetEndPoint().X, beam.GetEndPoint().Y, SHIFT);
                    curveArray.Append(Line.CreateBound(beam.GetStartPoint(), beam.GetEndPoint()));
                }
                edges.Add(curveArray);
            }

            using (Transaction trans = new Transaction(revitDoc))
            {
                trans.Start("Create Floors");
                List<List<CurveArray>> Group = GetInnerRegion(edges);
                foreach (List<CurveArray> floors in Group)
                {
                    Floor floor = revitDoc.Create.NewFloor(floors[0], floor_type, targetLevel, IsStructural);
                    XYZ translationVec = new XYZ(0, 0, SHIFT);
                    floor.Location.Move(translationVec);

                    if (floors.Count > 1)
                    {
                        for (int i = 1; i < floors.Count; i++)
                        {
                            revitDoc.Regenerate();
                            var res = revitDoc.Create.NewOpening(floor, floors[i], false);
                        }
                    }
                }
                trans.Commit();


            }



        }


        /// <summary>
        /// 母子區域處理
        /// </summary>
        /// <param name="Floors"></param>
        /// <returns></returns>
        private List<List<CurveArray>> GetInnerRegion(List<CurveArray> Floors)
        {
            List<int> flag = new List<int>();
            List<List<CurveArray>> Group = new List<List<CurveArray>>();
            for (int i = 0; i < Floors.Count; i++)
            {
                List<CurveArray> newGroup = new List<CurveArray>() { Floors[i] };
                for (int j = 0; j < Floors.Count; j++)
                {
                    if (i == j || flag.Contains(j)) continue;
                    if (IsInner(GetMaxRange(Floors[i]), GetMaxRange(Floors[j])))
                    {
                        flag.Add(j);
                        newGroup.Add(Floors[j]);
                        if (!flag.Contains(i)) flag.Add(i);
                    }
                }
                if (newGroup.Count != 1) Group.Add(newGroup);
            }

            for (int i = 0; i < Floors.Count; i++)
            {
                if (!flag.Contains(i))
                {
                    Group.Add(new List<CurveArray>() { Floors[i] });
                }
            }


            return Group;
        }
        
        /// <summary>
        /// 找出封閉曲線最大與最小邊界值
        /// </summary>
        /// <param name="curveArray"></param>
        /// <returns></returns>
        private List<XYZ> GetMaxRange(CurveArray curveArray)
        {
            List<Curve> curve = new List<Curve>();

            for (int i = 0; i < curveArray.Size; i++)
            {
                curve.Add(curveArray.get_Item(i));
            }

            double min_x = curve.Min(t => t.GetEndPoint(0).X);
            double max_x = curve.Max(t => t.GetEndPoint(0).X);
            double min_y = curve.Min(t => t.GetEndPoint(0).Y);
            double max_y = curve.Max(t => t.GetEndPoint(0).Y);


            return new List<XYZ>() { new XYZ(min_x, min_y, 0), new XYZ(max_x, max_y, 0) };
        }

        /// <summary>
        /// 判斷range_2 是否在 range_1裡面
        /// </summary>
        /// <param name="range_1"></param>
        /// <param name="range_2"></param>
        /// <returns></returns>
        private bool IsInner(List<XYZ> range_1, List<XYZ> range_2)
        {
            XYZ p1 = range_1[0];
            XYZ p2 = range_2[0];
            XYZ p3 = range_1[1];
            XYZ p4 = range_2[1];

            if (p2.X > p1.X && p2.Y > p1.Y && p3.X > p4.X && p3.Y > p4.Y)
            {
                return true;
            }

            return false;

        }


        /// <summary>
        /// 依照是否指定CAD圖層來擷取封閉區域
        /// </summary>
        /// <param name="IndicatiedLayers"></param>
        /// <param name="Layers"></param>
        /// <param name="FloorRegions"></param>
        /// <returns></returns>
        private List<List<LINE>> RegionProcess(bool IndicatiedLayers, string Layers, Dictionary<string, List<List<LINE>>> FloorRegions)
        {
            List<List<LINE>> RESULT = new List<List<LINE>>();
            if (IndicatiedLayers)
            {
                return FloorRegions[Layers];
            }

            foreach (KeyValuePair<string, List<List<LINE>>> item in FloorRegions)
            {
                foreach (List<LINE> Regions in item.Value)
                {
                    RESULT.Add(Regions);
                }
            }

            return RESULT;

        }


    }
}
