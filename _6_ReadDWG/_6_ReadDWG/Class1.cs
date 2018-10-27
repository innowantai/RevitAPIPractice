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
        public Document revitDoc;
        public UIDocument uidoc;
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

            Level lv = revitDoc.GetElement(new ElementId(2607)) as Level;
            AssignDefaultTypeToColumn(revitDoc);
            //ProcessVisible(uidoc);

            return Result.Succeeded;
        }


        private void AssignDefaultTypeToColumn(Document document)
        {
            ElementId defaultTypeId = document.GetDefaultFamilyTypeId(new ElementId(BuiltInCategory.OST_StructuralColumns));

            if (defaultTypeId != ElementId.InvalidElementId)
            {
                FamilySymbol defaultType = document.GetElement(defaultTypeId) as FamilySymbol;
                if (defaultType != null)
                {

                }
            }


            FilteredElementCollector viewCollector = new FilteredElementCollector(document);
            viewCollector.OfClass(typeof(Level));

            foreach (Element viewElement in viewCollector)
            {
                Level ll = (Level)viewElement;
                TaskDialog.Show("1", ll.Name); 
            }

            //Dictionary<string, List<FamilySymbol>> winFamilyTypes = FindFamilyTypes(revitDoc, BuiltInCategory.OST_StructuralColumns);
            //Dictionary<string, List<FamilySymbol>> winFamilyTypes = FindFamilyTypes(revitDoc, BuiltInCategory.OST_StructuralFraming);

            //using (StreamWriter sw = new StreamWriter(@"C:\Users\Wantai\Desktop\Output.txt", true))
            //{
            //    sw.WriteLine(string.Format("{0} Window Families were found in {1} ms.", winFamilyTypes.Count(), 123));
            //    foreach (KeyValuePair<string, List<FamilySymbol>> entry in winFamilyTypes)
            //    {
            //        sw.WriteLine(string.Format("\tFamily name: {0}", entry.Key));
            //        foreach (FamilySymbol item in entry.Value)
            //        {
            //            sw.WriteLine(string.Format("\t\tSymbol/Type name: {0}", item.Name));
            //        }
            //    }
            //}
        }

        public static Dictionary<string, List<FamilySymbol>> FindFamilyTypes(Document doc, BuiltInCategory cat)
        {
            return new FilteredElementCollector(doc)
                            .WherePasses(new ElementClassFilter(typeof(FamilySymbol)))
                            .WherePasses(new ElementCategoryFilter(cat))
                            .Cast<FamilySymbol>() 
                            .GroupBy(e => e.Family.Name)
                            .ToDictionary(e => e.Key, e => e.ToList());
        }



        FamilyInstance CreateColumn(Autodesk.Revit.DB.Document document, Level level)
        {
            // Get a Column type from Revit
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralColumns);
            FamilySymbol columnType = collector.FirstElement() as FamilySymbol;

            FamilyInstance instance = null;
            if (null != columnType)
            {
                // Create a column at the origin
                XYZ origin = new XYZ(0, 0, 0);

                instance = document.Create.NewFamilyInstance(origin, columnType, level, StructuralType.Column);
            }

            return instance;
        }





        /// <summary>
        /// Pick a DWG import instance, extract polylines 
        /// from it visible in the current view and create
        /// filled regions from them.
        /// </summary>
        public void ProcessVisible(UIDocument uidoc)
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
                            if (obj is PolyLine | obj is Arc)
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
                    foreach (var obj in visible_dwg_geo)
                    {  

                        if (obj is PolyLine)
                        {
                            // Create loops for detail region 
                            var poly = obj as PolyLine; 
                            var points = poly.GetCoordinates();
                            
                        }
                        else if(obj is Arc)
                        {
                            var Arc = obj as Arc;
                            XYZ pp = Arc.Center;


                        }
                        
                    }
                }
            }
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

