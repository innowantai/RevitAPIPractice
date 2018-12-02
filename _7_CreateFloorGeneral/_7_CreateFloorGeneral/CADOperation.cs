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

namespace _7_CreateFloorGeneral
{
    class CADOperation
    {
        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public Dictionary<string, List<Geometry>> GeneralCAD(UIDocument uidoc)
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
            Dictionary<string, List<Geometry>> res = new Dictionary<string, List<Geometry>>();
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
                    List<Geometry> tmp = new List<Geometry>();
                    foreach (var obj in visible_dwg_geo)
                    {
                        var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                        string layerName = gStyle.GraphicsStyleCategory.Name;

                        tmp = res.ContainsKey(layerName) ? res[layerName] : new List<Geometry>();
                        if (obj is PolyLine)
                        {
                            // Create loops for detail region 
                            var poly = obj as PolyLine;
                            var points = poly.GetCoordinates();
                            for (int kk = 0; kk < points.Count - 1; kk++)
                            {
                                LINE ll = new LINE(points[kk], points[kk + 1]);
                                ll.Type = "Line";
                                tmp.Add(ll);
                            }
                        }
                        else if (obj is Line)
                        {
                            var line = obj as Line;
                            LINE ll = new LINE(line.GetEndPoint(0), line.GetEndPoint(1));
                            ll.Type = "Line";
                            tmp.Add(ll);
                        }
                        else if (obj is Arc)
                        {
                            var Arc = obj as Arc;
                            Curve cc = Arc as Curve;
                            if (cc.IsBound)
                            {
                                ARC aa = new ARC(Arc.Center, Arc.Radius, Arc.XDirection, Arc.YDirection, Arc.Normal, cc.GetEndPoint(0), cc.GetEndPoint(1),true);
                                aa.Type = "Arc";
                                tmp.Add(aa);
                            }
                            else
                            {
                                ARC aa = new ARC(Arc.Center, Arc.Radius, Arc.XDirection, Arc.YDirection, Arc.Normal, new XYZ(), new XYZ(), false);
                                aa.Type = "Arc";
                                tmp.Add(aa);
                            }
                        }
                        res[layerName] = tmp;
                    }
                }
            }

            return res;
        }

    }

    /// <summary>
    /// Allow selection of elements of type T only.
    /// </summary>
    class JtElementsOfClassSelectionFilter<T> : ISelectionFilter where T : Element
    {
        public bool AllowElement(Element e)
        {
            return e is T;
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return true;
        }
    }
}
