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
using Autodesk.Revit.DB.Architecture;
using System.Diagnostics;
using Autodesk.Revit.DB.IFC;

namespace _6_ReadDWG
{
    public class CreateWallByRoomEdgeOrClickWall
    {
        public FindRevitElements RevFind = new FindRevitElements();
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


        public void Main_CreateWallByRoom(Document revitDoc, UIDocument uidoc, UIApplication uiapp, Application app)
        {
            /// 取得所有Room
            List<Element> ROOMs = RevFind.GetDocRooms(revitDoc);
            /// 取得所有樓層
            List<Level> levels = RevFind.GetLevels(revitDoc);
            /// 取得所有牆的種類 
            List<Element> WallTypes = RevFind.GetDocWallTypes(revitDoc);

            Form_CreateFloorByRoomEdges Form = new Form_CreateFloorByRoomEdges(levels, WallTypes);
            Form.ShowDialog();

            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {

                Reference r = uidoc.Selection.PickObject(ObjectType.Element, "Select a wall");
                Element e = uidoc.Document.GetElement(r);
                Room room = e as Room;
                List<Room> ROOM = new List<Room>() { room };

                //Level level = levels[Form.cmbBaseLevels.SelectedIndex];
                WallType WallType = WallTypes[Form.cmbFloorTypes.SelectedIndex] as WallType;
                Dictionary<string, List<List<LINE>>> Dict_ROOMs = new Dictionary<string, List<List<LINE>>>();
                Dictionary<string, List<List<Wall>>> Dict_Walls = new Dictionary<string, List<List<Wall>>>();
                RoomAndWallEdgeProcessing(uiapp, app, revitDoc, ROOM, ref Dict_ROOMs, ref Dict_Walls);

                List<List<List<Curve>>> WallCollection = new List<List<List<Curve>>>();
                List<List<Wall>> ClosedWallCollection = new List<List<Wall>>();
                foreach (KeyValuePair<string, List<List<Wall>>> item in Dict_Walls)
                {
                    foreach (List<Wall> WallList in item.Value)
                    {
                        ClosedWallCollection.Add(WallList);

                        //foreach (Wall wall in WallList)
                        //{
                        //    List<List<Curve>> WallEdge = GetWallInformation(uidoc, revitDoc, app, wall, WallType);
                        //    WallCollection.Add(WallEdge);
                        //}
                    }
                }

                CreateWall(uidoc, revitDoc, app, ClosedWallCollection, WallType);
            }
        }

        private void CreateWall(UIDocument uidoc, Document revitDoc, Application app, List<List<Wall>> RoomsClosedWalls, WallType WallType)
        {

            using (Transaction tx = new Transaction(revitDoc))
            {
                tx.Start("Wall Profile");

                foreach (List<Wall> Wall_C in RoomsClosedWalls)
                {

                    List<WallCollection> WallCurves = new List<WallCollection>();
                    foreach (Wall wall in Wall_C)
                    {

                        WallCollection Walls = GetWallInformation(uidoc, revitDoc, app, wall, WallType);
                        WallCurves.Add(Walls);
                    }
                     
                    int kk = 0;
                    foreach (WallCollection wallC in WallCurves)
                    {
                        try
                        {
                            Wall wall = Wall.Create(revitDoc, wallC.GetAllEdge(), false);
                            wall.WallType = WallType;
                            Element e = wall as Element;
                            Parameter PP = e.get_Parameter(BuiltInParameter.DOOR_NUMBER);
                            PP.Set(kk.ToString());




                        }
                        catch (Exception)
                        {
                        }
                        kk++;
                    }



                }

                tx.Commit();
            }
        }


         



        private Dictionary<string, List<List<LINE>>> RoomAndWallEdgeProcessing(UIApplication uiapp,
                                                                                Application app,
                                                                                Document revitDoc,
                                                                                List<Room> Room_Level,
                                                                                ref Dictionary<string, List<List<LINE>>> Dict_ROOMs,
                                                                                ref Dictionary<string, List<List<Wall>>> Dict_Walls)
        {

            View3D view3d = new FilteredElementCollector(revitDoc)
                             .OfClass(typeof(View3D))
                             .Cast<View3D>()
                             .FirstOrDefault<View3D>(e => e.Name.Equals("{3D}"));

            /// 擷取各個房間邊界
            int kk = 0;
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

                        Element e = GetElementByRay(uiapp, revitDoc, view3d, seg.GetCurve());
                        Wall wall = e as Wall;
                        walls.Add(wall);
                    }
                    Region.Add(roomEdge);
                    WALLs.Add(walls);
                }
                Dict_ROOMs[kk.ToString() + "_" + room.Name] = Region;
                Dict_Walls[kk.ToString() + "_" + room.Name] = WALLs;
                kk++;
            }
            return Dict_ROOMs;
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





        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        ///////////////////////////////////////// Click To Create //////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 

        public void Main_CreateWallByClickWall(Document revitDoc, UIDocument uidoc, UIApplication uiapp, Application app)
        {
            /// 取得所有樓層
            List<Level> levels = RevFind.GetLevels(revitDoc);
            /// 取得所有牆的種類 
            List<Element> WallTypes = RevFind.GetDocWallTypes(revitDoc);

            Form_CreateWallByRoomEdgeOrClickWall Form = new Form_CreateWallByRoomEdgeOrClickWall(levels, WallTypes);
            Form.ShowDialog();

            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                double SHIFT = Convert.ToDouble(Form.txtShift.Text) / 304.8;
                Level level = levels[Form.cmbBaseLevels.SelectedIndex];
                WallType WallType = WallTypes[Form.cmbFloorTypes.SelectedIndex] as WallType;

                Reference r = uidoc.Selection.PickObject(ObjectType.Element, "Select a wall");
                Element e = uidoc.Document.GetElement(r);
                Wall wall = e as Wall;
                CreateWall(uidoc, revitDoc, app, new List<List<Wall>>() { new List<Wall>() { wall } }, WallType);
            }
        }



        private WallCollection GetWallInformation(UIDocument uidoc, Document revitDoc, Application app, Wall wall, WallType WallType)
        {

            Autodesk.Revit.Creation.Document credoc = revitDoc.Create;
            Autodesk.Revit.Creation.Application creapp = app.Create;
            View view = revitDoc.ActiveView;

            ElementType type = WallType as ElementType;
            Parameter b = type.get_Parameter((BuiltInParameter.WALL_ATTR_WIDTH_PARAM));
            double width = b.AsDouble() / 2;

            IList<Reference> sideFaces = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);

            Element e2 = revitDoc.GetElement(sideFaces[0]);

            Face face = e2.GetGeometryObjectFromReference(sideFaces[0]) as Face;

            // The normal of the wall external face.
            XYZ normal = face.ComputeNormal(new UV(0, 0));

            // Offset curve copies for visibility.
            Transform offset = Transform.CreateTranslation(width * normal);

            // If the curve loop direction is counter-
            // clockwise, change its color to RED.


            // Get edge loops as curve loops.
            IList<CurveLoop> curveLoops = face.GetEdgesAsCurveLoops();

            // ExporterIFCUtils class can also be used for 
            // non-IFC purposes. The SortCurveLoops method 
            // sorts curve loops (edge loops) so that the 
            // outer loops come first. 
            IList<IList<CurveLoop>> curveLoopLoop = ExporterIFCUtils.SortCurveLoops(curveLoops);
            List<List<Curve>> Walls = new List<List<Curve>>();
            WallCollection WCCC = new WallCollection();
            foreach (IList<CurveLoop> curveLoops2 in curveLoopLoop)
            {
                foreach (CurveLoop curveLoop2 in curveLoops2)
                {
                    // Check if curve loop is counter-clockwise. 

                    CurveArray curves = creapp.NewCurveArray();
                    List<Curve> CC = new List<Curve>();
                    foreach (Curve curve in curveLoop2)
                    {
                        curves.Append(curve.CreateTransformed(offset));
                        CC.Add(curve.CreateTransformed(offset));
                    }
                    // Create model lines for an curve loop. 
                    Walls.Add(CC);
                    WCCC.AddWall(CC);
                }
            }

            return WCCC;
        }





        //private Dictionary<string, List<List<LINE>>> RoomAndWallEdgeProcessing(UIApplication uiapp,
        //                                                                        Application app,
        //                                                                        Document revitDoc,
        //                                                                        List<Element> ROOMs,
        //                                                                        Level targetLevel,
        //                                                                        ref Dictionary<string, List<List<LINE>>> Dict_ROOMs,
        //                                                                        ref Dictionary<string, List<List<Wall>>> Dict_Walls)
        //{

        //    View3D view3d = new FilteredElementCollector(revitDoc)
        //                     .OfClass(typeof(View3D))
        //                     .Cast<View3D>()
        //                     .FirstOrDefault<View3D>(e => e.Name.Equals("{3D}"));

        //    /// 過濾出指定樓層的房間或面機
        //    var Roomsbylevel_filcol = new FilteredElementCollector(revitDoc) //search only in this level
        //    .OfClass(typeof(SpatialElement))                                 //get all rooms
        //    .Cast<SpatialElement>()                                          //cast results to SpatialElements 
        //    .Where(o => o.LevelId == targetLevel.Id);                        //search by the above mentioned Level

        //    ///判斷是否為房間且存在
        //    List<Room> Room_Level = new List<Room>();
        //    foreach (SpatialElement item in Roomsbylevel_filcol)
        //    {
        //        if (item.GetType().Name == "Room" && item.Location != null)
        //        {
        //            Room rr = item as Room;
        //            Room_Level.Add(rr);
        //        }
        //    }

        //    /// 擷取各個房間邊界
        //    int kk = 0;
        //    foreach (Room room in Room_Level)
        //    {
        //        List<List<LINE>> Region = new List<List<LINE>>();
        //        List<List<Wall>> WALLs = new List<List<Wall>>();
        //        IList<IList<BoundarySegment>> loops = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
        //        foreach (IList<BoundarySegment> loop in loops)
        //        {
        //            List<LINE> roomEdge = new List<LINE>();
        //            List<Wall> walls = new List<Wall>();
        //            foreach (BoundarySegment seg in loop)
        //            {
        //                Curve cc = seg.GetCurve();
        //                Line curve = cc as Line;
        //                LINE line = new LINE(curve.GetEndPoint(0), curve.GetEndPoint(1));
        //                line.Name = room.Name;
        //                roomEdge.Add(line);

        //                Element e = GetElementByRay(uiapp, revitDoc, view3d, seg.GetCurve());
        //                Wall wall = e as Wall;
        //                walls.Add(wall);
        //            }
        //            Region.Add(roomEdge);
        //            WALLs.Add(walls);
        //        }
        //        Dict_ROOMs[kk.ToString() + "_" + room.Name] = Region;
        //        Dict_Walls[kk.ToString() + "_" + room.Name] = WALLs;
        //        kk++;
        //    }
        //    return Dict_ROOMs;
        //}



        ///// <summary>
        ///// 建立樓板
        ///// </summary>
        ///// <param name="revitDoc"></param>
        ///// <param name="targetLevel"></param>
        ///// <param name="NewBeamGroup_"></param>
        ///// <param name="floor_type"></param>
        ///// <param name="SHIFT"></param>
        ///// <param name="IsStructural"></param>
        //private void CreateWall_ByRoom(Document revitDoc,
        //                                Level targetLevel,
        //                                Dictionary<string, List<List<LINE>>> NewBeamGroup_,
        //                                Dictionary<string, List<List<Wall>>> Dict_Walls,
        //                                WallType floor_type,
        //                                double SHIFT,
        //                                bool IsStructural)
        //{
        //    Dictionary<string, List<List<CurveArray>>> GROUP = new Dictionary<string, List<List<CurveArray>>>();
        //    foreach (KeyValuePair<string, List<List<LINE>>> NewBeamGroup in NewBeamGroup_)
        //    {
        //        List<CurveArray> edges = new List<CurveArray>();
        //        foreach (List<LINE> Beams in NewBeamGroup.Value)
        //        {
        //            CurveArray curveArray = new CurveArray();
        //            foreach (LINE beam in Beams)
        //            {
        //                XYZ startPoint = new XYZ(beam.GetStartPoint().X, beam.GetStartPoint().Y, SHIFT);
        //                XYZ endPoint = new XYZ(beam.GetEndPoint().X, beam.GetEndPoint().Y, SHIFT);
        //                curveArray.Append(Line.CreateBound(beam.GetStartPoint(), beam.GetEndPoint()));
        //            }
        //            edges.Add(curveArray);
        //        }
        //        GROUP[NewBeamGroup.Key] = GetInnerRegion(edges);
        //    }
        //}





        //private void GetWallInformationForTestUse(UIDocument uidoc, Document revitDoc, Application app, Wall wall, WallType WallType)
        //{

        //    Autodesk.Revit.Creation.Document credoc = revitDoc.Create;
        //    Autodesk.Revit.Creation.Application creapp = app.Create;
        //    View view = revitDoc.ActiveView;

        //    ElementType type = WallType as ElementType;
        //    Parameter b = type.get_Parameter((BuiltInParameter.WALL_ATTR_WIDTH_PARAM));
        //    double width = b.AsDouble() / 2;


        //    using (Transaction tx = new Transaction(revitDoc))
        //    {
        //        tx.Start("Wall Profile");
        //        // Get the external wall face for the profile
        //        IList<Reference> sideFaces = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);

        //        Element e2 = revitDoc.GetElement(sideFaces[0]);

        //        Face face = e2.GetGeometryObjectFromReference(sideFaces[0]) as Face;

        //        // The normal of the wall external face.
        //        XYZ normal = face.ComputeNormal(new UV(0, 0));

        //        // Offset curve copies for visibility.
        //        Transform offset = Transform.CreateTranslation(width * normal);

        //        // If the curve loop direction is counter-
        //        // clockwise, change its color to RED.

        //        Color colorRed = new Color(255, 0, 0);

        //        // Get edge loops as curve loops.
        //        IList<CurveLoop> curveLoops = face.GetEdgesAsCurveLoops();

        //        // ExporterIFCUtils class can also be used for 
        //        // non-IFC purposes. The SortCurveLoops method 
        //        // sorts curve loops (edge loops) so that the 
        //        // outer loops come first. 
        //        IList<IList<CurveLoop>> curveLoopLoop = ExporterIFCUtils.SortCurveLoops(curveLoops);
        //        List<List<Curve>> Walls = new List<List<Curve>>();
        //        foreach (IList<CurveLoop> curveLoops2 in curveLoopLoop)
        //        {
        //            foreach (CurveLoop curveLoop2 in curveLoops2)
        //            {
        //                // Check if curve loop is counter-clockwise.

        //                bool isCCW = curveLoop2.IsCounterclockwise(normal);

        //                CurveArray curves = creapp.NewCurveArray();
        //                List<Curve> CC = new List<Curve>();
        //                foreach (Curve curve in curveLoop2)
        //                {
        //                    curves.Append(curve.CreateTransformed(offset));
        //                    CC.Add(curve.CreateTransformed(offset));
        //                }
        //                // Create model lines for an curve loop. 
        //                Walls.Add(CC);
        //                IList<Curve> curvesList = CC.ToList();
        //                CurveLoop curvesLoop = CurveLoop.Create(curvesList);
        //                Plane plane = curvesLoop.GetPlane();

        //                SketchPlane sketchPlane = SketchPlane.Create(revitDoc, plane);

        //                ModelCurveArray curveElements
        //                  = credoc.NewModelCurveArray(curves,
        //                    sketchPlane);

        //                //Wall www = Wall.Create(revitDoc, curvesList, false);

        //                if (isCCW)
        //                {
        //                    foreach (ModelCurve mcurve in curveElements)
        //                    {
        //                        OverrideGraphicSettings overrides
        //                          = view.GetElementOverrides(
        //                            mcurve.Id);

        //                        overrides.SetProjectionLineColor(
        //                          colorRed);

        //                        view.SetElementOverrides(
        //                          mcurve.Id, overrides);
        //                    }
        //                }
        //            }
        //        }


        //        List<LINE> WALLLINE = new List<LINE>();
        //        List<Curve> wallC = new List<Curve>();
        //        foreach (List<Curve> item in Walls)
        //        {
        //            foreach (Curve cc in item)
        //            {
        //                wallC.Add(cc);
        //                WALLLINE.Add(new LINE(cc.GetEndPoint(0), cc.GetEndPoint(1)));
        //            }
        //        }


        //        Wall createWall = Wall.Create(revitDoc, wallC, false);
        //        createWall.WallType = WallType;



        //        tx.Commit();
        //    }
        //}

    }



    public class WallCollection
    {
        public List<Curve> FatherWall;
        public List<List<Curve>> SubWall;

        public WallCollection()
        {
            this.FatherWall = new List<Curve>();
            this.SubWall = new List<List<Curve>>();
        }

        public void AddWall(List<Curve> wall)
        {
            if (this.FatherWall.Count == 0)
            {
                this.FatherWall = wall;
            }
            else
            {
                this.SubWall.Add(wall);
            }
        }


        private List<LINE> GetFatherWallByLINE()
        {
            List<LINE> RESULT = new List<LINE>();
            foreach (Curve item in this.FatherWall)
            {
                Line line = item as Line;
                LINE newLINE = new LINE(line.Origin, line.Direction, line.Length);
                RESULT.Add(newLINE);
            }
            return RESULT;
        }

        /// <summary>
        /// 矩形牆左下開始逆時針四個座標
        /// </summary>
        /// <returns></returns>
        public List<XYZ> GetFatherWallRange()
        {
            List<LINE> RESULT = this.GetFatherWallByLINE();
            double min_x = RESULT.Min(t => t.GetStartPoint().X);
            double min_y = RESULT.Min(t => t.GetStartPoint().Y);
            double min_z = RESULT.Min(t => t.GetStartPoint().Z);
            double max_x = RESULT.Max(t => t.GetStartPoint().X);
            double max_y = RESULT.Max(t => t.GetStartPoint().Y);
            double max_z = RESULT.Max(t => t.GetStartPoint().Z);
            XYZ Coor1 = new XYZ(min_x, min_y, min_z);
            XYZ Coor2 = new XYZ(max_x, max_y, min_z);
            XYZ Coor3 = new XYZ(max_x, max_y, max_z);
            XYZ Coor4 = new XYZ(min_x, min_y, max_z);

            return new List<XYZ>() { Coor1, Coor2, Coor3, Coor4 };
        }


        public List<Curve> GetAllEdge()
        {
            List<Curve> resule = new List<Curve>();
            foreach (Curve item in this.FatherWall)
            {
                resule.Add(item);
            }

            foreach (List<Curve> item in this.SubWall)
            {
                foreach (var cc in item)
                {
                    resule.Add(cc);
                }
            }

            return resule;
        }

        public void ResetFatherWall(List<Curve> wall)
        {
            this.FatherWall = wall;
        }
    }

}
