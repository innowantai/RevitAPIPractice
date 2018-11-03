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

/// <summary>
/// The targets will be solved :
/// 1. If the family instance was not be used, that will appear the message of "familySymbol is not activate" 
///    Solved : Create the Transaction to active indiacted FamilySymbol before use it 
/// 2. Need to get the LevelId of the GeometryInstance
///    Solved : will be ------
/// </summary>


namespace _6_ReadDWG
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Application revitApp;
        public UIDocument uidoc;
        public static Document revitDoc;
        public static string startPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public FindRevitElements RevFind = new FindRevitElements();
        public CreateObjects RevCreate;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            string getPath = Path.Combine(startPath, "CAD_REVIT_DATA");
            revitDoc = commandData.Application.ActiveUIDocument.Document;
            uidoc = commandData.Application.ActiveUIDocument;

            ElementId ele = null;
            Selection selection = uidoc.Selection;
            ICollection<ElementId> element = selection.GetElementIds();
            foreach (ElementId eleID in element)
            {
                ele = eleID;
                break;
            }

            RevCreate = new CreateObjects(revitDoc);


            Dictionary<string, List<XYZ[]>> res = GeneralCAD(uidoc);
            List<XYZ[]> LINES = res["梁"];
            // Main_API();
             
            int[] is_pickup = new int[LINES.Count];
            List<List<XYZ[]>> Collect = new List<List<XYZ[]>>();
            for (int i = 0; i < LINES.Count; i++)
            {
                if(is_pickup[i] == 1) continue;

                XYZ[] baseLine = LINES[i];
                List<XYZ[]> tmpData = new List<XYZ[]>();
                tmpData.Add(baseLine);
                int j = 0;

                while (j < LINES.Count)
                {
                    XYZ[] cmpLine = LINES[j];
                    if (is_pickup[j] == 1 || j == i)
                    {
                        j = j + 1;
                        continue;
                    }
                    if (cmpLine[0].X == baseLine[1].X && cmpLine[0].Y == baseLine[1].Y)
                    {
                        baseLine = cmpLine;
                        tmpData.Add(baseLine);
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (cmpLine[1].X == baseLine[1].X && cmpLine[1].Y == baseLine[1].Y)
                    {
                        baseLine = new XYZ[] { cmpLine[1], cmpLine[0] };
                        tmpData.Add(baseLine);
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (tmpData[0][0].X == cmpLine[0].X && tmpData[0][0].Y == cmpLine[0].Y)
                    {
                        tmpData.Insert(0, new XYZ[] { cmpLine[1], cmpLine[0] });
                        is_pickup[j] = 1;
                        j = 0;
                    }
                    else if (tmpData[0][0].X == cmpLine[1].X && tmpData[0][0].Y == cmpLine[1].Y)
                    {
                        tmpData.Insert(0, cmpLine);
                        is_pickup[j] = 1;
                        j = 0;
                    } 
                    else
                    {
                        j = j + 1;
                    } 
                }
                 

                if (tmpData.Count != 1)
                {
                    Collect.Add(tmpData);
                    is_pickup[i] = 1;
                }

            }

            return Result.Succeeded;
        }



        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public Dictionary<string, List<XYZ[]>> GeneralCAD(UIDocument uidoc)
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
            Dictionary<string, List<XYZ[]>> res = new Dictionary<string, List<XYZ[]>>();
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
                    List<XYZ[]> tmp = new List<XYZ[]>();
                    foreach (var obj in visible_dwg_geo)
                    {
                        var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                        string layerName = gStyle.GraphicsStyleCategory.Name;

                        tmp = res.ContainsKey(layerName) ? res[layerName] : new List<XYZ[]>();
                        if (obj is PolyLine)
                        {
                            // Create loops for detail region 
                            var poly = obj as PolyLine;
                            var points = poly.GetCoordinates();
                            for (int kk = 0; kk < points.Count - 1; kk++)
                            {
                                XYZ[] pp = new XYZ[2];
                                tmp.Add(new XYZ[2] { points[kk], points[kk + 1] });
                            }
                        }
                        else if (obj is Line)
                        {
                            XYZ[] pp = new XYZ[2];
                            var line = obj as Line;
                            tmp.Add(new XYZ[2] { line.GetEndPoint(0),
                                                 line.GetEndPoint(1) });
                        }
                        //else if (obj is Arc)
                        //{
                        //    var Arc = obj as Arc;
                        //    pp[0] = Arc.Center;
                        //    pp[1] = Arc.Center;
                        //} 
                        res[layerName] = tmp;
                    }
                }
            }

            return res;
        }





        private void Main_API()
        {
            Dictionary<string, List<XYZ[]>> res = ProcessVisible(uidoc);
            Dictionary<string, List<FamilySymbol>> colFamilyTypes = RevFind.GetDocColumnsTypes(revitDoc);
            Dictionary<string, List<FamilySymbol>> beamFamilyTypes = RevFind.GetDocBeamTypes(revitDoc);
            List<Level> levels = RevFind.GetLevels(revitDoc);
            Form1 form = new Form1(colFamilyTypes, beamFamilyTypes, levels, res);
            form.ShowDialog();

            if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (form.returnCol)
                {
                    int CaseIndex = 0;
                    foreach (XYZ[] pp in res[form.returnCADLayers[CaseIndex]])
                    {
                        RevCreate.CreateColumn(form.returnType[CaseIndex], form.returnBaseLevel[CaseIndex], form.returnTopLevel[CaseIndex], pp);
                    }
                }

                if (form.returnBeam)
                {
                    int CaseIndex = 1;
                    foreach (XYZ[] pp in res[form.returnCADLayers[CaseIndex]])
                    {
                        RevCreate.CreateBeam(form.returnType[CaseIndex], form.returnBaseLevel[CaseIndex], pp);
                    }
                }

            }
        }


        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public Dictionary<string, List<XYZ[]>> ProcessVisible(UIDocument uidoc)
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
            Dictionary<string, List<XYZ[]>> res = new Dictionary<string, List<XYZ[]>>();
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
                    List<XYZ[]> tmp = new List<XYZ[]>();
                    foreach (var obj in visible_dwg_geo)
                    {
                        var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                        string layerName = gStyle.GraphicsStyleCategory.Name;
                        XYZ[] pp = new XYZ[2];
                        if (obj is PolyLine)
                        {
                            // Create loops for detail region 
                            var poly = obj as PolyLine;
                            var points = poly.GetCoordinates();
                            pp[0] = points[0];
                            pp[1] = points[2];

                        }
                        else if (obj is Arc)
                        {
                            var Arc = obj as Arc;
                            pp[0] = Arc.Center;
                            pp[1] = Arc.Center;
                        }
                        else if (obj is Line)
                        {
                            var line = obj as Line;
                            pp[0] = line.GetEndPoint(0);
                            pp[1] = line.GetEndPoint(1);
                        }
                        tmp = res.ContainsKey(layerName) ? res[layerName] : new List<XYZ[]>();
                        tmp.Add(pp);
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

