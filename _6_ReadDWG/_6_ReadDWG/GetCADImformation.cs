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
    public class GetCADImformation
    {
        private bool IsArc;
        private bool IsLine;
        private bool IsPloyLine;
        public XYZ Origin;
        public double Scale;
        public List<XYZ> Rotation_Dir;
        public double Rotation;
        public Dictionary<string, List<LINE>> LayersAndGeometries;
        public Dictionary<string, List<List<LINE>>> LayersAndClosedRegions;

        public GetCADImformation(bool isArc, bool isLine, bool isPloyLine)
        {
            this.IsArc = isArc;
            this.IsLine = isLine;
            this.IsPloyLine = isPloyLine;
            this.LayersAndGeometries = new Dictionary<string, List<LINE>>();
            this.LayersAndClosedRegions = new Dictionary<string, List<List<LINE>>>();
        }

        public void CADProcessing(UIDocument uidoc)
        {
            GeneralCAD(uidoc);
            ReSortDatasByLayersName();
        }

        /// <summary>
        /// 依照圖層名稱重新排列
        /// </summary>
        private void ReSortDatasByLayersName()
        {
            /// 幾何部分處理
            Dictionary<string, List<LINE>> CADGeometry = new Dictionary<string, List<LINE>>();
            var Layers = this.LayersAndGeometries.OrderBy(t => t.Key).ToList();

            foreach (KeyValuePair<string, List<LINE>> item in Layers)
            {
                CADGeometry[item.Key] = this.LayersAndGeometries[item.Key];
            }
            this.LayersAndGeometries.Clear();
            this.LayersAndGeometries = CADGeometry;


            /// Hash部分處理
            Dictionary<string, List<List<LINE>>> CADGeometry_Region = new Dictionary<string, List<List<LINE>>>();
            var Layers_Region = this.LayersAndClosedRegions.OrderBy(t => t.Key).ToList();

            foreach (KeyValuePair<string, List<List<LINE>>> item in Layers_Region)
            {
                CADGeometry_Region[item.Key] = this.LayersAndClosedRegions[item.Key];
            }
            this.LayersAndClosedRegions.Clear();
            this.LayersAndClosedRegions = CADGeometry_Region;
        }


        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        private void GeneralCAD(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View active_view = doc.ActiveView;
            List<GeometryObject> visible_dwg_geo = new List<GeometryObject>();

            // 點選匯入之CAD 
            Reference r = uidoc.Selection.PickObject(ObjectType.Element, new JtElementsOfClassSelectionFilter<ImportInstance>());
            var import = doc.GetElement(r) as ImportInstance;

            // 取得幾何
            var ge = import.get_Geometry(new Options());
            foreach (GeometryObject go in ge)
            {
                if (go is GeometryInstance)
                {
                    GeometryInstance gi = go as GeometryInstance;
                    GeometryElement ge2 = gi.GetInstanceGeometry();
                    this.Origin = gi.Transform.Origin;
                    this.Rotation_Dir = new List<XYZ>() { gi.Transform.BasisX, gi.Transform.BasisY, gi.Transform.BasisZ };
                    this.Rotation = GetSita(this.Rotation_Dir[0]);
                    this.Scale = gi.Transform.Scale;

                    if (ge2 != null)
                    {

                        foreach (var obj in ge2)
                        {
                            /// 如果Object為幾何物件
                            if ((obj is PolyLine && IsPloyLine) || (obj is Arc && IsArc) || (obj is Line && IsLine))
                            {

                                GraphicsStyle gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                                // Check if the layer is visible in the view. 
                                if (!active_view.GetCategoryHidden(gStyle.GraphicsStyleCategory.Id))
                                {
                                    visible_dwg_geo.Add(obj);
                                }
                            }
                            else if (obj is Solid)
                            {
                                try
                                {
                                    Solid soild = obj as Solid;
                                    string layerName = "";
                                    foreach (PlanarFace planarFace in soild.Faces)
                                    {
                                        EdgeArrayArray Eaa = planarFace.EdgeLoops;
                                        foreach (EdgeArray edgearray in Eaa)
                                        {
                                            List<LINE> edges = new List<LINE>();
                                            foreach (Edge edge in edgearray)
                                            {
                                                var gStyle = doc.GetElement(edge.GraphicsStyleId) as GraphicsStyle;
                                                layerName = gStyle.GraphicsStyleCategory.Name;
                                                Line curve = edge.AsCurve() as Line;
                                                LINE newLine = new LINE(curve.GetEndPoint(0), curve.GetEndPoint(1));
                                                edges.Add(newLine);
                                            }

                                            List<List<LINE>> EDGEs = this.LayersAndClosedRegions.Keys.Contains(layerName) ?
                                                                     this.LayersAndClosedRegions[layerName] : new List<List<LINE>>();
                                            EDGEs.Add(edges);
                                            this.LayersAndClosedRegions[layerName] = EDGEs;
                                        }
                                    }
                                }
                                catch (Exception)
                                { 
                                }
                            }
                        }
                    }
                }
            }


            /// 取得各格圖層的幾何資訊
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
                            for (int kk = 0; kk < points.Count - 1; kk++)
                            {

                                if (GetDistance(points[kk], points[kk+1]) != 0)
                                { 
                                    LINE newLine = new LINE(points[kk], points[kk + 1]);
                                    newLine.Name = "Polyline";
                                    tmp.Add(newLine);
                                }
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

            this.LayersAndGeometries = res;
            try
            {
            }
            catch (Exception e)
            {

            }
        }



        /// <summary>
        ///  取得角度
        /// </summary>
        /// <param name="Rotation"></param>
        /// <returns></returns>
        private double GetSita(XYZ Rotation)
        {
            double Sita = 0;
            Sita = Math.Atan(Math.Abs(Rotation.Y) / Math.Abs(Rotation.X));

            if (Rotation.X < 0 && Rotation.Y > 0)
            {
                return Math.PI - Sita; //// II
            }
            else if (Rotation.X < 0 && Rotation.Y < 0)
            {
                return Math.PI + Sita; /// III
            }
            else if (Rotation.X > 0 && Rotation.Y < 0)
            {
                return Math.PI * 2 - Sita;  /// IV
            }

            return Sita; /// I
        }


        private double GetDistance(XYZ Point1, XYZ Point2)
        {
            return Math.Sqrt((Point1.X - Point2.X) * (Point1.X - Point2.X) +
                             (Point1.Y - Point2.Y) * (Point1.Y - Point2.Y)
                             );
        }

    }

}
