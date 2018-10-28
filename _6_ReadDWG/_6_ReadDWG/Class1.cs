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

namespace _6_ReadDWG
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Application revitApp;
        public UIDocument uidoc;
        public static Document revitDoc;
        public static string startPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        

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


            UIApplication uiapp = commandData.Application.ActiveUIDocument.Application; 


            Dictionary<string, List<XYZ[]>> res = ProcessVisible(uidoc); 
            Dictionary<string, List<FamilySymbol>> colFamilyTypes = FindFamilyTypes(revitDoc, BuiltInCategory.OST_StructuralColumns);
            Dictionary<string, List<FamilySymbol>> beamFamilyTypes = FindFamilyTypes(revitDoc, BuiltInCategory.OST_StructuralFraming);
            List<Level> levels = FindLevels(revitDoc);
            Form1 form = new Form1(colFamilyTypes, beamFamilyTypes, levels, res);
            form.ShowDialog();

            if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                int CaseIndex = 0;
                foreach (XYZ[] pp in res[form.returnCADLayers[CaseIndex]])
                {
                    CreateColumn(form.returnType[CaseIndex], form.returnBaseLevel[CaseIndex], form.returnTopLevel[CaseIndex], pp);
                }
                CaseIndex = 1;
                foreach (XYZ[] pp in res[form.returnCADLayers[CaseIndex]])
                {
                    CreateBeam(form.returnType[CaseIndex], form.returnBaseLevel[CaseIndex], pp);
                } 

            }




            return Result.Succeeded;
        }

         
        public void ShowForm(UIApplication uiapp)
        {

        }
         

        private static void CreateColumn(FamilySymbol Type, Level baseLevel, Level topLevel,XYZ[] points)
        {
            using (Transaction trans = new Transaction(revitDoc))
            {
                trans.Start("Create Column"); 
                FamilyInstance familyInstance = null;
                XYZ point = new XYZ((points[0].X + points[1].X)/2 , (points[0].Y + points[1].Y)/2, 0);
                XYZ botPoint = new XYZ(point.X, point.Y, baseLevel.Elevation);
                XYZ topPoint = new XYZ(point.X, point.Y, topLevel.Elevation );
                familyInstance = revitDoc.Create.NewFamilyInstance(Line.CreateBound(botPoint, topPoint), Type, baseLevel, StructuralType.Column);
                trans.Commit(); 
            }
        } 

        private static void CreateBeam(FamilySymbol Type, Level baseLevel, XYZ[] points)
        {
            using (Transaction trans = new Transaction(revitDoc))
            {
                trans.Start("Create Beam");
                FamilyInstance familyInstance = null;
                XYZ p1 = new XYZ(points[0].X, points[0].Y, baseLevel.Elevation);
                XYZ p2 = new XYZ(points[1].X, points[1].Y, baseLevel.Elevation);
                familyInstance = revitDoc.Create.NewFamilyInstance(Line.CreateBound(p1, p2), Type, baseLevel, StructuralType.Beam);
                trans.Commit();
            }
        }



        /// <summary>
        /// Get Family Types
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        public static Dictionary<string, List<FamilySymbol>> FindFamilyTypes(Document doc, BuiltInCategory cat)
        {
            return new FilteredElementCollector(doc)
                            .WherePasses(new ElementClassFilter(typeof(FamilySymbol)))
                            .WherePasses(new ElementCategoryFilter(cat))
                            .Cast<FamilySymbol>() 
                            .GroupBy(e => e.Family.Name)
                            .ToDictionary(e => e.Key, e => e.ToList());
        }


        /// <summary>
        /// Get All Level
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static List<Level> FindLevels(Document document)
        {

            FilteredElementCollector viewCollector = new FilteredElementCollector(document);
            viewCollector.OfClass(typeof(Level));
            List<Level> ResLevel = new List<Level>();
            foreach (Element viewElement in viewCollector)
            {
                Level ll = (Level)viewElement; 
                ResLevel.Add(ll);
            }
            return ResLevel;
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
            List<GeometryObject> visible_dwg_geo  = new List<GeometryObject>();

            // Pick Import Instance 
            Reference r = uidoc.Selection.PickObject(ObjectType.Element,  new JtElementsOfClassSelectionFilter<ImportInstance>()); 
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
                        else if(obj is Arc)
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
    class JtElementsOfClassSelectionFilter<T>
          : ISelectionFilter where T : Element
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

