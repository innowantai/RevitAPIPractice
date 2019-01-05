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
            //Dictionary<string, List<LINE>> res_ = GeneralCAD(uidoc);
            //Dictionary<string, List<LINE>> res = GetPolylineAndLineClosedRegion(res_);




            Dictionary<string, List<LINE>> CADGeometry = null;
            CADGeometry = GeneralCAD(uidoc);


            Dictionary<string, List<FamilySymbol>> LightFamilyTypes = CatchLightFamilyType(RevFind.GetDocLightTypes(revitDoc));
            List<Level> levels = RevFind.GetLevels(revitDoc);
            Form_CreateLight Form = new Form_CreateLight(LightFamilyTypes, levels, CADGeometry);
            Form.ShowDialog();

            string RadioCase = Form.radCircle.Checked == true ? "Circle" : "Ployline";

            if (Form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                double SHIFT = Convert.ToDouble(Form.txtShift.Text) / 304.8;
                List<LINE> LINES_ = CADGeometry[Form.cmbColCADLayers.Text];
                List<LINE> LINES = GetPolylineAndLineClosedRegion(LINES_, RadioCase);
                List<XYZ> centerPoint = TakeOffSameLightPoint(LINES);

                foreach (XYZ pp in centerPoint)
                {
                    XYZ newPP = new XYZ(pp.X, pp.Y, SHIFT);
                    RevCreate.CreateLight(Form.returnType[0], Form.returnBaseLevel[0], newPP);
                }

            }
        }


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
                else if(RadioCase != "Circle")
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


        private Dictionary<string, List<FamilySymbol>> CatchLightFamilyType(Dictionary<string, List<FamilySymbol>> Types)
        {
            Dictionary<string, List<FamilySymbol>> LightType = new Dictionary<string, List<FamilySymbol>>();
            foreach (KeyValuePair<string, List<FamilySymbol>> item in Types)
            {
                List<FamilySymbol> newList = new List<FamilySymbol>();
                if (item.Key.Contains('燈') || item.Key.Contains("Light") || item.Key.Contains("light") || item.Key.Contains("風口"))
                {
                    LightType[item.Key] = item.Value;
                }
            }

            return LightType;
        }

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
