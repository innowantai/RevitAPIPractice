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
    class CreateLightObject
    {


        public FindRevitElements RevFind = new FindRevitElements();
        public void Main_Create(Document revitDoc, UIDocument uidoc)
        {
            CreateObjects RevCreate = new CreateObjects(revitDoc);


            Dictionary<string, List<LINE>> CADGeometry = null;
            /// 讀取CAD所有圖層幾何資訊
            CADGeometry = GeneralCAD(uidoc);
            /// 若沒選擇CAD 則回傳
            if (CADGeometry == null) return;


            /// 取得Revit指定的FamilyTypes
            Dictionary<string, List<Dictionary<string, List<FamilySymbol>>>> LightFamilyTypes = CatchLightFamilyType(RevFind.GetDocLightTypes(revitDoc));
            /// 取得Revit所有樓層資訊
            List<Level> levels = RevFind.GetLevels(revitDoc);
            /// 建立Form物件
            Form_CreateLight Form = new Form_CreateLight(LightFamilyTypes, levels, CADGeometry);
            Form.ShowDialog();

            /// 確認是要用圓形或者是多邊形心中來建立物件
            string RadioCase = Form.radCircle.Checked == true ? "Circle" : "Ployline";

            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                /// 樓層偏移量
                double SHIFT = Convert.ToDouble(Form.txtShift.Text) / 304.8;
                /// 取得目標樓層的所有幾何資訊
                List<LINE> LINES_ = CADGeometry[Form.cmbColCADLayers.Text];
                /// 針對圓形或多邊形處理幾何資訊
                List<LINE> LINES = GetPolylineAndLineClosedRegion(LINES_, RadioCase);
                /// 將重複的點拿掉
                List<XYZ> centerPoint = TakeOffSameLightPoint(LINES);

                /// 激活FamilyType
                StartFamilyType(Form.returnType[0], revitDoc);

                /// 開始建立物件
                CreateLight(Form.returnType[0], Form.returnBaseLevel[0], revitDoc, centerPoint);

            }
        }

        public void StartFamilyType(FamilySymbol Type, Document revitDoc)
        {
            if (!Type.IsActive)
            {
                using (Transaction trans = new Transaction(revitDoc))
                {
                    trans.Start("Activate Family instance");
                    Type.Activate();
                    trans.Commit();
                }
            }
        }


        /// <summary>
        /// Create Column 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="baseLevel"></param>
        /// <param name="topLevel"></param>
        /// <param name="points"></param>
        public void CreateLight(FamilySymbol Type, Level baseLevel, Document revitDoc, List<XYZ> POINTs)
        {

            using (Transaction trans = new Transaction(revitDoc))
            {
                trans.Start("Create Family Instance");
                foreach (XYZ points in POINTs)
                {
                    FamilyInstance familyInstance = null;
                    familyInstance = revitDoc.Create.NewFamilyInstance(points, Type, baseLevel, StructuralType.NonStructural);
                }
                //familyInstance = revitDoc.Create.NewFamilyInstance(points, Type, StructuralType.NonStructural);
                trans.Commit();
            }
        }



        /// <summary>
        /// 族群處理
        /// </summary>
        /// <param name="Types"></param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, List<FamilySymbol>>>> CatchLightFamilyType(Dictionary<string, List<FamilySymbol>> Types)
        {
            Dictionary<string, List<Dictionary<string, List<FamilySymbol>>>> AllFamilyTypes = new Dictionary<string, List<Dictionary<string, List<FamilySymbol>>>>();
            
            foreach (KeyValuePair<string, List<FamilySymbol>> item in Types)
            {
                Dictionary<string, List<FamilySymbol>> LightType = new Dictionary<string, List<FamilySymbol>>();
                LightType[item.Key] = item.Value;
                 
                string KEY = item.Value[0].Category.Name.ToString();

                if (AllFamilyTypes.Keys.Contains(KEY))
                {
                    List<Dictionary<string, List<FamilySymbol>>> tmpData = AllFamilyTypes[KEY];
                    tmpData.Add(LightType);
                }
                else
                {
                    AllFamilyTypes[KEY] = new List<Dictionary<string, List<FamilySymbol>>>() { LightType };
                }

                //if (item.Key.Contains('燈') || item.Key.Contains("Light") || item.Key.Contains("light") || item.Key.Contains("風口"))
                //{
                //    LightType[item.Key] = item.Value;
                //    //string test = item.Value[0].Category.Name.ToString(); 
                //}
            }
             
           return AllFamilyTypes;
        }



        /// <summary>
        /// 多邊形中心處理
        /// </summary>
        /// <param name="Lines"></param>
        /// <param name="RadioCase"></param>
        /// <returns></returns>
        private List<LINE> GetPolylineAndLineClosedRegion(List<LINE> Lines, string RadioCase)
        {
            List<LINE> RESULT = new List<LINE>();

            List<LINE> newResult = new List<LINE>();
            List<LINE> processLines = new List<LINE>();
            foreach (LINE line in Lines)
            {
                if (line.Name == "Circle" && RadioCase == "Circle")
                {
                    newResult.Add(line);
                }
                else if (RadioCase != "Circle")
                {
                    processLines.Add(line);
                }
            }
            if (RadioCase == "Circle") return newResult;


            List<int> flag = new List<int>() { };
            for (int i = 0; i < processLines.Count; i++)
            {
                int j = 0;
                flag.Add(i);
                List<LINE> polyline = new List<LINE>() { processLines[i] };
                while (j < processLines.Count)
                {
                    if (flag.Contains(j))
                    {
                        j++;
                        continue;
                    }

                    foreach (LINE tmpLine in polyline)
                    {
                        if (IsConnected(tmpLine, processLines[j]))
                        {
                            polyline.Add(processLines[j]);
                            flag.Add(j);
                            j = -1;
                            break;
                        }
                    }
                    j++;
                }

                if (polyline.Count != 1)
                {
                    double newX = polyline.Sum(tt => tt.GetStartPoint().X) / polyline.Count;
                    double newY = polyline.Sum(tt => tt.GetStartPoint().Y) / polyline.Count;
                    XYZ newPoint = new XYZ(newX, newY, 0);
                    LINE newLine = new LINE(newPoint, newPoint, 10);
                    newResult.Add(newLine);
                }
            }

            RESULT = newResult;


            return RESULT;
        }



        /// <summary>
        /// 移除相同點
        /// </summary>
        /// <param name="LINES"></param>
        /// <returns></returns>
        private List<XYZ> TakeOffSameLightPoint(List<LINE> LINES)
        {
            List<int> flag = new List<int>();
            for (int i = 0; i < LINES.Count; i++)
            {
                for (int j = i; j < LINES.Count; j++)
                {
                    if (i == j || flag.Contains(j)) continue;
                    if (IsSamePoint(LINES[i].GetStartPoint(), LINES[j].GetStartPoint()))
                    {
                        flag.Add(j);
                    }
                }
            }

            List<XYZ> centerPoint = new List<XYZ>();
            for (int i = 0; i < LINES.Count; i++)
            {
                if (!flag.Contains(i))
                {
                    centerPoint.Add(LINES[i].GetStartPoint());
                }
            }

            return centerPoint;

        }

        /// <summary>
        /// 是否為相同點
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private bool IsSamePoint(XYZ point1, XYZ point2)
        {
            int IGPoint = 2;
            if (Math.Round(point1.X, IGPoint) == Math.Round(point2.X, IGPoint) &&
                Math.Round(point1.Y, IGPoint) == Math.Round(point2.Y, IGPoint) &&
                Math.Round(point1.Z, IGPoint) == Math.Round(point2.Z, IGPoint))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 兩線段是否連接
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private bool IsConnected(LINE line1, LINE line2)
        {
            if (IsSamePoint(line1.GetStartPoint(), line2.GetStartPoint()) ||
                IsSamePoint(line1.GetEndPoint(), line2.GetEndPoint()) ||
                IsSamePoint(line1.GetStartPoint(), line2.GetEndPoint()) ||
                IsSamePoint(line1.GetEndPoint(), line2.GetStartPoint()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public Dictionary<string, List<LINE>> GeneralCAD(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View active_view = doc.ActiveView;
            List<GeometryObject> visible_dwg_geo = new List<GeometryObject>();

            try
            {  
                // Pick Import Instance 
                Reference r = uidoc.Selection.PickObject(ObjectType.Element, new JtElementsOfClassSelectionFilter<ImportInstance>());
                var import = doc.GetElement(r) as ImportInstance;

                // Get Geometry 
                var ge = import.get_Geometry(new Options());
                foreach (var go in ge)
                {
                    if (go is GeometryInstance)
                    {
                        var gi = go as GeometryInstance;
                        var ge2 = gi.GetInstanceGeometry();
                        if (ge2 != null)
                        {
                            foreach (var obj in ge2)
                            {
                                // Only work on PolyLines 
                                if (obj is PolyLine | obj is Arc | obj is Line)
                                {

                                    // Use the GraphicsStyle to get the 
                                    // DWG layer linked to the Category 
                                    // for visibility.

                                    var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                                    // Check if the layer is visible in the view.

                                    if (!active_view.GetCategoryHidden(gStyle.GraphicsStyleCategory.Id))
                                    {
                                        visible_dwg_geo.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }


                // Do something with the info 
                Dictionary<string, List<LINE>> res = new Dictionary<string, List<LINE>>();
                if (visible_dwg_geo.Count > 0)
                {
                    // Retrieve first filled region type 
                    var filledType = new FilteredElementCollector(doc)
                      .WhereElementIsElementType()
                      .OfClass(typeof(FilledRegionType))
                      .OfType<FilledRegionType>()
                      .First();

                    using (var t = new Transaction(doc))
                    {
                        t.Start("ProcessDWG");
                        List<LINE> tmp = new List<LINE>();
                        foreach (var obj in visible_dwg_geo)
                        {
                            var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                            string layerName = gStyle.GraphicsStyleCategory.Name;

                            tmp = res.ContainsKey(layerName) ? res[layerName] : new List<LINE>();
                            //if (obj is PolyLine)
                            //{
                            //    // Create loops for detail region 
                            //    var poly = obj as PolyLine;
                            //    var points = poly.GetCoordinates();
                            //    for (int kk = 0; kk < points.Count - 1; kk++)
                            //    {
                            //        tmp.Add(new LINE(points[kk], points[kk + 1]));
                            //    }
                            //}
                            //else if (obj is Line)
                            //{
                            //    var line = obj as Line;
                            //    tmp.Add(new LINE(line.GetEndPoint(0),
                            //                         line.GetEndPoint(1)));
                            //}
                            //else if (obj is Arc)
                            //{
                            //    var Arc = obj as Arc;
                            //    pp[0] = Arc.Center;
                            //    pp[1] = Arc.Center;
                            //} 
                            if (obj is Arc)
                            {
                                var Arc = obj as Arc;
                                XYZ newPoint = new XYZ(Arc.Center.X, Arc.Center.Y, 0);
                                LINE newLine = new LINE(newPoint, newPoint, 10);
                                newLine.Name = "Circle";
                                tmp.Add(newLine);
                            }
                            if (obj is PolyLine)
                            {
                                // Create loops for detail region 
                                var poly = obj as PolyLine;
                                var points = poly.GetCoordinates();
                                //List<XYZ> POINTs = points.ToList();
                                //double meanX = POINTs.Sum(tt => tt.X) / POINTs.Count;
                                //double meanY = POINTs.Sum(tt => tt.Y) / POINTs.Count;
                                //XYZ newPoint = new XYZ(meanX, meanY, 0);
                                //LINE newLine = new LINE(newPoint, newPoint, 10);
                                //newLine.Name = "PolyLine-" + POINTs.Count.ToString();
                                //tmp.Add(newLine);
                                for (int kk = 0; kk < points.Count - 1; kk++)
                                {
                                    LINE newLine = new LINE(points[kk], points[kk + 1]);
                                    newLine.Name = "Polyline";
                                    tmp.Add(newLine);
                                }
                            }
                            else if (obj is Line)
                            {
                                var line = obj as Line;
                                LINE newLine = new LINE(line.GetEndPoint(0), line.GetEndPoint(1));
                                newLine.Name = "Line";
                                tmp.Add(newLine);
                            }
                            res[layerName] = tmp;
                        }
                    }
                }

                return res;
            }
            catch (Exception)
            {

                return null;
            }
        }


        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public Dictionary<string, List<LINE>> ProcessVisible(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View active_view = doc.ActiveView;
            List<GeometryObject> visible_dwg_geo = new List<GeometryObject>();

            // Pick Import Instance 
            Reference r = uidoc.Selection.PickObject(ObjectType.Element, new JtElementsOfClassSelectionFilter<ImportInstance>());
            var import = doc.GetElement(r) as ImportInstance;

            // Get Geometry 
            var ge = import.get_Geometry(new Options());
            foreach (var go in ge)
            {
                if (go is GeometryInstance)
                {
                    var gi = go as GeometryInstance;
                    var ge2 = gi.GetInstanceGeometry();
                    if (ge2 != null)
                    {
                        foreach (var obj in ge2)
                        {
                            // Only work on PolyLines 
                            if (obj is PolyLine | obj is Arc | obj is Line)
                            {

                                // Use the GraphicsStyle to get the 
                                // DWG layer linked to the Category 
                                // for visibility.

                                var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                                // Check if the layer is visible in the view.

                                if (!active_view.GetCategoryHidden(gStyle.GraphicsStyleCategory.Id))
                                {
                                    visible_dwg_geo.Add(obj);
                                }
                            }
                        }
                    }
                }
            }

            // Do something with the info 
            Dictionary<string, List<LINE>> res = new Dictionary<string, List<LINE>>();
            if (visible_dwg_geo.Count > 0)
            {
                // Retrieve first filled region type 
                var filledType = new FilteredElementCollector(doc)
                  .WhereElementIsElementType()
                  .OfClass(typeof(FilledRegionType))
                  .OfType<FilledRegionType>()
                  .First();

                using (var t = new Transaction(doc))
                {
                    t.Start("ProcessDWG");
                    List<LINE> tmp = new List<LINE>();
                    foreach (var obj in visible_dwg_geo)
                    {
                        var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                        string layerName = gStyle.GraphicsStyleCategory.Name;
                        LINE pp = null;
                        if (obj is PolyLine)
                        {
                            // Create loops for detail region 
                            var poly = obj as PolyLine;
                            var points = poly.GetCoordinates();
                            pp = new LINE(points[0], points[2]);

                        }
                        else if (obj is Arc)
                        {
                            var Arc = obj as Arc;
                            pp = new LINE(Arc.Center, Arc.Center);
                        }
                        else if (obj is Line)
                        {
                            var line = obj as Line;
                            pp = new LINE(line.GetEndPoint(0), line.GetEndPoint(1));
                        }
                        tmp = res.ContainsKey(layerName) ? res[layerName] : new List<LINE>();
                        tmp.Add(pp);
                        res[layerName] = tmp;
                    }
                }
            }

            return res;
        }


    }
}
