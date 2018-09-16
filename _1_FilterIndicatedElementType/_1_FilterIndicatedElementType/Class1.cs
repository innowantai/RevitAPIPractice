using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


/// <summary>
/// 1.Get the element for indicated type from entired revit document, 
///     In this case : Indicated "Wall" type 
///     using parameterset to get element parameter IEnumerator
/// 2.Get the element for indicated type from seleted element
/// </summary>
namespace _1_FilterIndicatedElementType
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {

        /// <summary>
        /// Find All Exterior wall from selected element
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string info = "";
            UIDocument uidoc = commandData.Application.ActiveUIDocument; 
            if (uidoc != null)
            { 
                ICollection<ElementId> element = uidoc.Selection.GetElementIds();
                foreach (ElementId ele in element)
                {
                    
                    Wall wall = uidoc.Document.GetElement(ele) as Wall;
                    if (wall != null)
                    {
                        info += wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble().ToString() + "\n";
                        //info += wall.GetTypeId().ToString() + "\n"; 
                    } 
                }

                TaskDialog.Show("revit", info);
            }
            else
            {
                TaskDialog.Show("result", "You do not select any element");
            }
            return Result.Succeeded;
        }


        /// <summary>
        /// Find All Exterior wall from Entired document
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        //public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        //{              
        //    Document document = commandData.Application.ActiveUIDocument.Document; 
        //    FilteredElementCollector filteredElements = new FilteredElementCollector(document);
        //    ElementClassFilter classFilter = new ElementClassFilter(typeof(Wall));
        //    filteredElements = filteredElements.WherePasses(classFilter);   

        //    string info = "The entried Revit exterior wall results : \n";
        //    foreach (Wall wall in filteredElements)
        //    {
        //        var functionParameter = wall.WallType.get_Parameter(BuiltInParameter.FUNCTION_PARAM);
        //        if (functionParameter != null && functionParameter.StorageType == StorageType.Integer)
        //        {
        //            if (functionParameter.AsInteger() == (int)WallFunction.Exterior)
        //            {
        //                ParameterSet paras = wall.Parameters;
        //                foreach (Parameter pa in paras)
        //                { 
        //                    info += pa.Definition.Name.ToString() + "\n";  
        //                }                        
        //            }
        //        }
        //    }
        //    TaskDialog.Show("revit", info); 
        //    return Result.Succeeded;
        //}


    }
}
