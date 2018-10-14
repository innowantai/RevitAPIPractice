using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Exceptions;

namespace _2_GetSelectedElementParameter
{

    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string doorTypeName = "0762 x 2032mm";
            FamilySymbol doorType = null;
            Document RevitDoc = commandData.Application.ActiveUIDocument.Document;
            ElementFilter doorCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            ElementFilter familySymbolFilter = new ElementClassFilter(typeof(FamilySymbol));
            LogicalAndFilter andFilter = new LogicalAndFilter(doorCategoryFilter, familySymbolFilter);
            FilteredElementCollector doorSymbol = new FilteredElementCollector(RevitDoc);
            doorSymbol.WherePasses(andFilter);

            bool symbolFound = false;
            foreach (FamilySymbol element in doorSymbol)
            {
                if (element.Name == doorTypeName)
                {
                    //TaskDialog.Show("revit", element.ToString());
                    symbolFound = true;
                    doorType = element;
                    break;
                }

            }


            if (symbolFound)
            {
                ElementFilter wallFilter = new ElementClassFilter(typeof(Wall));
                FilteredElementCollector filteredElements = new FilteredElementCollector(RevitDoc);
                filteredElements.WherePasses(wallFilter);
                Wall wall = null;
                Line line = null;
                List<Wall> wallList = new List<Wall>();
                List<Line> lineList = new List<Line>();
                string res = "";
                foreach (Wall element in filteredElements)
                {
                    LocationCurve locationCurve = element.Location as LocationCurve;
                    if (locationCurve != null)
                    {
                        line = locationCurve.Curve as Line;
                        res += line.GetEndPoint(0).ToString() + " " + line.GetEndPoint(1).ToString() + "\n";
                        wallList.Add(element);
                        lineList.Add(line);
                        wall = element;
                        break;
                    }
                }
                TaskDialog.Show("revit", res);


                if (wall != null)
                {

                    using (var transaction = new Transaction(RevitDoc))
                    {
                        XYZ midPoint = (line.GetEndPoint(0) + line.GetEndPoint(1)) / 2;
                        Level wallLevel = RevitDoc.GetElement(wall.LevelId) as Level;
                        FamilyInstance door = RevitDoc.Create.NewFamilyInstance(midPoint, doorType, wall, wallLevel, 0); 
                    } 
                    //TaskDialog.Show("Succeed", door.Id.ToString());
                    // Trace.WriteLine("Door Created" + door.Id.ToString());
                    //TaskDialog.Show("res", midPoint + "\n" + doorType.Id + "\n" + wall.Id + "\n" + wallLevel.Id + "\n" + Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                }

            }

            return Result.Succeeded;
        }

    }
}
