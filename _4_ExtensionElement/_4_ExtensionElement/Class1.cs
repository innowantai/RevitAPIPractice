using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace _4_ExtensionElement
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            createExtention(commandData,);

            return Result.Succeeded;
        }


        void createExtention(Autodesk.Revit.DB.Document Revitdoc,Autodesk.Revit.ApplicationServices.Application RevitApp)
        {
            Document familyDoc = RevitApp.NewFamilyDocument(@"C:\ProgramData\Autodesk\RVT 2018\Family Templates\Chinese\公制常规模型.rft");
            using (Transaction transaction = new Transaction(familyDoc))
            {

                transaction.Start("Create family");
                CurveArray curveArray = new CurveArray(); 
                curveArray.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(5, 0, 0)));
                curveArray.Append(Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)));
                curveArray.Append(Line.CreateBound(new XYZ(5, 5, 0), new XYZ(5, 5, 0)));
                curveArray.Append(Line.CreateBound(new XYZ(0, 5, 0), new XYZ(5, 0, 0)));
               // CurveArrArray curveArrArray = new CurveArrArray();
               // familyDoc.FamilyCreate.NewExtrusion(true, curveArrArray, SketchPlane.Create(familyDoc, RevitApp.Create.NewPointOnPlane(new XYZ(0, 0, 1), XYZ.Zero, 10)));
            }


        }
    }
}
