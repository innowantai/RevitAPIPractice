using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using FamilyLabsCS;

namespace _5_VideoFamily
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        public Application _rvtApp;
        public Document _rvtDoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _rvtApp = commandData.Application.Application;
            _rvtDoc = commandData.Application.ActiveUIDocument.Document;

            if (!isRightTemplate(BuiltInCategory.OST_Columns))
            {
                Util.ErrorMsg("Please open Metric Column.rft");
                return Result.Failed;
            }

            try
            {
                using (Transaction tran = new Transaction(_rvtDoc, "Add Type"))
                {
                    tran.Start();
                    addTypes();
                    tran.Commit();
                }

            }
            catch (Exception e)
            {

                TaskDialog.Show("error",e.Message);
            }

            return Result.Succeeded;
        }


        bool isRightTemplate(BuiltInCategory targetCategory)
        {
            // This command works in the context of family editor only.
            //
            if (!_rvtDoc.IsFamilyDocument)
            {
                Util.ErrorMsg("This command works only in the family editor.");
                return false;
            }

            // Check the template for an appropriate category here if needed.
            //
            Category cat = _rvtDoc.Settings.Categories.get_Item(targetCategory);
            if (_rvtDoc.OwnerFamily == null)
            {
                Util.ErrorMsg("This command only works in the family context.");
                return false;
            }
            if (!cat.Id.Equals(_rvtDoc.OwnerFamily.FamilyCategory.Id))
            {
                Util.ErrorMsg("Category of this family document does not match the context required by this command.");
                return false;
            }

            // if we come here, we should have a right one.
            return true;
        }


        void addTypes()
        {
            // addType(name, Width, Depth)
            //
            addType("600x900", 600.0, 900.0);
            addType("1000x300", 1000.0, 300.0);
            addType("600x600", 600.0, 600.0);
        }

        void addType(string name, double w, double d)
        {
            // get the family manager from the current doc
            FamilyManager pFamilyMgr = _rvtDoc.FamilyManager;

            // add new types with the given name
            //
            FamilyType type1 = pFamilyMgr.NewType(name);

            // look for 'Width' and 'Depth' parameters and set them to the given value
            //
            // first 'Width'
            //
            FamilyParameter paramW = pFamilyMgr.get_Parameter("Width");
            double valW = w;
            if (paramW != null)
            {
                pFamilyMgr.Set(paramW, valW);
            }

            // same idea for 'Depth'
            //
            FamilyParameter paramD = pFamilyMgr.get_Parameter("Depth");
            double valD = d;
            if (paramD != null)
            {
                pFamilyMgr.Set(paramD, valD);
            }
        }


    }
}
