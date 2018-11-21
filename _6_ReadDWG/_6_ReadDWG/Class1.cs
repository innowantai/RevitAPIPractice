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
/// 2018/11/17
/// The targets will be solved :
/// 1. If the family instance was not be used, that will appear the message of "familySymbol is not activate" 
///    Solved : Create the Transaction to active indiacted FamilySymbol before use it 
/// 2. Need to get the LevelId of the GeometryInstance
///    Solved : Beams LevelId have been got by compare with the value of Location of z
/// 3. Automatically Create floors by beam                                                       
///    will do : find the outline of the columne to open floor                                        ((2018/11/18
/// 4. Above Case all for H and V direction only 
/// 5. 樓板建立all case有些問題
/// 6. 梁的偏移計算方式可能需要修改，取的其GeometryInstance去轉換而不是用檔名去換算
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


            CreateBeamsAndColumns Creation = new CreateBeamsAndColumns();
            Creation.Main_Create(revitDoc, uidoc);


            

            



            return Result.Succeeded;
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

