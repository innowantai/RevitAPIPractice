using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
namespace _0_ShowSelectedElement
{
     
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument; 
            Selection selection = uidoc.Selection;  
            ICollection<ElementId> collection = selection.GetElementIds(); 
            if (0 == collection.Count)
            {
                TaskDialog.Show("Revit", "You do not select any element");

            }
            else
            {
                string info = "You selected element is : ";
                foreach (ElementId elem in collection)
                {
                    // If want to get the element type that need to use the method of uidoc.Document.GetElement
                    info += uidoc.Document.GetElement(elem) + "\n";
                    // If want to get element ID that just use elem.InterValue
                    //            info += elem.IntegerValue + "\n";
                }
                TaskDialog.Show("Revit", info);
            }

            return Result.Succeeded;
        }

        //public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        //{
        //    UIDocument uidoc = commandData.Application.ActiveUIDocument;
        //    Selection selection = uidoc.Selection;
        //    ICollection<ElementId> collection = selection.GetElementIds();

        //    if (0 == collection.Count)
        //    {
        //        TaskDialog.Show("Revit", "You do not select any element");

        //    }
        //    else
        //    {
        //        string info = "You selected elements ID is : \n";
        //        foreach (ElementId elem in collection)
        //        {
        //            info += elem.IntegerValue + "\n";
        //        }
        //        TaskDialog.Show("Revit", info);
        //    }

        //    return Result.Succeeded;
        //}
    }
}
