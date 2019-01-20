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
using Autodesk.Revit.DB.Architecture;
using System.Diagnostics;
using Autodesk.Revit.DB.IFC;

namespace _6_ReadDWG
{
    public class CreateFloorByCADHashRegion
    {
        public FindRevitElements RevFind = new FindRevitElements();
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

       



        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////// Floor_Part ////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 


        /// <summary>
        /// 由房間區域建立樓板
        /// </summary>
        /// <param name="revitDoc"></param>
        /// <param name="uidoc"></param>
        /// <param name="uiapp"></param>
        /// <param name="app"></param>
        public void Main_CreateByRoom(Document revitDoc, UIDocument uidoc, UIApplication uiapp, Application app)
        {
            /// 取得所有Room
            List<Element> ROOMs = RevFind.GetDocRooms(revitDoc);
            /// 取得所有樓層
            List<Level> levels = RevFind.GetLevels(revitDoc);
            /// 取得所有板的種類 
            List<Element> floorTypes = RevFind.GetDocFloorTypes(revitDoc);

            Form_CreateFloorByRoomEdges Form = new Form_CreateFloorByRoomEdges(levels, floorTypes);
            Form.ShowDialog();

            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                double SHIFT = Convert.ToDouble(Form.txtShift.Text) / 304.8;
                bool IsStructural = Form.chIsStructural.Checked ? true : false;
                Level level = levels[Form.cmbBaseLevels.SelectedIndex];
                FloorType floorType = floorTypes[Form.cmbFloorTypes.SelectedIndex] as FloorType;
                Dictionary<string, List<List<LINE>>> Region = RoomEdgeProcessing(revitDoc, ROOMs, level);
                CreateFloor_ByRoom(revitDoc, level, Region, floorType, SHIFT, IsStructural);
            }
        }

        /// <summary>
        /// 建立樓板
        /// </summary>
        /// <param name="revitDoc"></param>
        /// <param name="targetLevel"></param>
        /// <param name="NewBeamGroup_"></param>
        /// <param name="floor_type"></param>
        /// <param name="SHIFT"></param>
        /// <param name="IsStructural"></param>
        private void CreateFloor_ByRoom(Document revitDoc, Level targetLevel, Dictionary<string, List<List<LINE>>> NewBeamGroup_, FloorType floor_type, double SHIFT, bool IsStructural)
        {
            Dictionary<string, List<List<CurveArray>>> GROUP = new Dictionary<string, List<List<CurveArray>>>();
            foreach (KeyValuePair<string, List<List<LINE>>> NewBeamGroup in NewBeamGroup_)
            {
                List<CurveArray> edges = new List<CurveArray>();
                foreach (List<LINE> Beams in NewBeamGroup.Value)
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
                GROUP[NewBeamGroup.Key] = GetInnerRegion(edges);
            }


            using (Transaction trans = new Transaction(revitDoc))
            {
                trans.Start("Create Floors");
                foreach (KeyValuePair<string, List<List<CurveArray>>> GG in GROUP)
                {
                    foreach (List<CurveArray> floors in GG.Value)
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

                        // Element e = floor as Element;
                        // Parameter PP = e.get_Parameter(BuiltInParameter.DOOR_NUMBER);
                        // PP.Set(GG.Key);
                    }
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 取得房間邊緣線
        /// </summary>
        /// <param name="revitDoc"></param>
        /// <param name="ROOMs"></param>
        /// <param name="targetLevel"></param>
        /// <returns></returns>
        private Dictionary<string, List<List<LINE>>> RoomEdgeProcessing(Document revitDoc, List<Element> ROOMs, Level targetLevel)
        {

            View3D view3d = new FilteredElementCollector(revitDoc)
                             .OfClass(typeof(View3D))
                             .Cast<View3D>()
                             .FirstOrDefault<View3D>(e => e.Name.Equals("{3D}"));

            /// 過濾出指定樓層的房間或面機
            var Roomsbylevel_filcol = new FilteredElementCollector(revitDoc) //search only in this level
            .OfClass(typeof(SpatialElement))                                 //get all rooms
            .Cast<SpatialElement>()                                          //cast results to SpatialElements 
            .Where(o => o.LevelId == targetLevel.Id);                        //search by the above mentioned Level

            ///判斷是否為房間且存在
            List<Room> Room_Level = new List<Room>();
            foreach (SpatialElement item in Roomsbylevel_filcol)
            {
                if (item.GetType().Name == "Room" && item.Location != null)
                {
                    Room rr = item as Room;
                    Room_Level.Add(rr);
                }
            }

            /// 擷取各個房間邊界
            int kk = 0;
            Dictionary<string, List<List<LINE>>> Dict_ROOMs = new Dictionary<string, List<List<LINE>>>();
            foreach (Room room in Room_Level)
            {
                List<List<LINE>> Region = new List<List<LINE>>();
                List<List<Wall>> WALLs = new List<List<Wall>>();
                IList<IList<BoundarySegment>> loops = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                foreach (IList<BoundarySegment> loop in loops)
                {
                    List<LINE> roomEdge = new List<LINE>();
                    List<Wall> walls = new List<Wall>();
                    foreach (BoundarySegment seg in loop)
                    {
                        Curve cc = seg.GetCurve();
                        Line curve = cc as Line;
                        LINE line = new LINE(curve.GetEndPoint(0), curve.GetEndPoint(1));
                        line.Name = room.Name;
                        roomEdge.Add(line);

                    }
                    Region.Add(roomEdge);
                    WALLs.Add(walls);
                }
                Dict_ROOMs[kk.ToString() + "_" + room.Name] = Region;
                kk++;
            }
            return Dict_ROOMs;
        }

        /// <summary>
        /// Return the neighbouring BIM element generating 
        /// the given room boundary curve c, assuming it
        /// is oriented counter-clockwise around the room
        /// if part of an interior loop, and vice versa.
        /// </summary>
        public Element GetElementByRay(UIApplication app, Document doc, View3D view3d, Curve c)
        {
            Element boundaryElement = null;

            // Tolerances

            const double minTolerance = 0.00000001;
            const double maxTolerance = 0.01;

            // Height of ray above room level:
            // ray starts from one foot above room level

            const double elevation = 1;

            // Ray starts not directly from the room border
            // but from a point offset slightly into it.

            const double stepInRoom = 0.1;

            // We could use Line.Direction if Curve c is a 
            // Line, but since c also might be an Arc, we 
            // calculate direction like this:

            XYZ lineDirection = (c.GetEndPoint(1) - c.GetEndPoint(0)).Normalize();

            XYZ upDir = elevation * XYZ.BasisZ;

            // Assume that the room is on the left side of 
            // the room boundary curve and wall on the right.
            // This is valid for both outer and inner room 
            // boundaries (outer are counter-clockwise, inner 
            // are clockwise). Start point is slightly inside 
            // the room, one foot above room level.

            XYZ toRoomVec = stepInRoom * GetLeftDirection(lineDirection);

            XYZ pointBottomInRoom = c.Evaluate(0.5, true) + toRoomVec;

            XYZ startPoint = pointBottomInRoom + upDir;

            // We are searching for walls only

            ElementFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);

            ReferenceIntersector intersector = new ReferenceIntersector(wallFilter, FindReferenceTarget.Element, view3d);

            // We don't want to find elements in linked files

            intersector.FindReferencesInRevitLinks = false;

            XYZ toWallDir = GetRightDirection(lineDirection);

            ReferenceWithContext context = intersector.FindNearest(startPoint, toWallDir);

            Reference closestReference = null;

            if (context != null)
            {
                if ((context.Proximity > minTolerance) && (context.Proximity < maxTolerance + stepInRoom))
                {
                    closestReference = context.GetReference();

                    if (closestReference != null)
                    {
                        boundaryElement = doc.GetElement(closestReference);
                    }
                }
            }
            return boundaryElement;
        }

        /// <summary>
        /// Return direction turning 90 degrees 
        /// left from given input vector.
        /// </summary>
        public XYZ GetLeftDirection(XYZ direction)
        {
            double x = -direction.Y;
            double y = direction.X;
            double z = direction.Z;
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Return direction turning 90 degrees 
        /// right from given input vector.
        /// </summary>
        public XYZ GetRightDirection(XYZ direction)
        {
            return GetLeftDirection(direction.Negate());
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        ///////////////////////////////////////////// CAD Part /////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
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
