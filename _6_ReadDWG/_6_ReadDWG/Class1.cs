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
            

            Dictionary<string, List<LINE>> res = GeneralCAD(uidoc);
            List<LINE> LINES = res["柱"];
            // Main_API();


            PreProcessing.ClassifyLines(LINES, out List<List<LINE>> Collect, 
                                 out List<LINE> H_Direction_Lines, 
                                 out List<LINE> V_Direction_Lines, 
                                 out List<LINE> Else_Direction_Lines);

            List<LINE> RES_BEAM = PreProcessing.BeamDrawLinesProcess(Collect,
                                     H_Direction_Lines,
                                     V_Direction_Lines );
            List<LINE> RES_COLUMN = PreProcessing.ColumnDrawLinesProcess(Collect);


            Dictionary<string, List<FamilySymbol>> beamFamilyTypes = RevFind.GetDocBeamTypes(revitDoc);
            List<Level> levels = RevFind.GetLevels(revitDoc);
            Dictionary<string, List<FamilySymbol>> colFamilyTypes = RevFind.GetDocColumnsTypes(revitDoc);
            Form1 form = new Form1(colFamilyTypes, beamFamilyTypes, levels, res);
            form.ShowDialog();
            foreach (LINE pp in RES_COLUMN)
            {
                RevCreate.CreateColumn(form.returnType[0], levels[0], levels[1], pp); 
                        
            }

            return Result.Succeeded;
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
                        if (obj is PolyLine)
                        {
                            // Create loops for detail region 
                            var poly = obj as PolyLine;
                            var points = poly.GetCoordinates();
                            for (int kk = 0; kk < points.Count - 1; kk++)
                            {
                                XYZ[] pp = new XYZ[2];
                                tmp.Add(new LINE( points[kk], points[kk + 1] ));
                            }
                        }
                        else if (obj is Line)
                        {
                            XYZ[] pp = new XYZ[2];
                            var line = obj as Line;
                            tmp.Add(new LINE( line.GetEndPoint(0),
                                                 line.GetEndPoint(1)));
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
            Dictionary<string, List<LINE>> res = ProcessVisible(uidoc);
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
                    foreach (LINE pp in res[form.returnCADLayers[CaseIndex]])
                    {
                        RevCreate.CreateColumn(form.returnType[CaseIndex], form.returnBaseLevel[CaseIndex], form.returnTopLevel[CaseIndex], pp);
                    }
                }

                if (form.returnBeam)
                {
                    int CaseIndex = 1;
                    foreach (LINE pp in res[form.returnCADLayers[CaseIndex]])
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

