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
///     Solved : GetBeamGroup 要弄成更General
/// 6. 梁的偏移計算方式可能需要修改，取得其GeometryInstance去轉換而不是用檔名去換算 
///     已解決(2018/11/25)
/// 7. 大型結構該樓層的梁高層可能不一樣，需分類
///     已解決(2018/11/25) --> 使用條件為梁的高度要相同
/// 
/// 8.擷取不到柱的Instance
///
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
            //Element e = revitDoc.GetElement(ele);

            ////// Processing Part
            //Parameter PP = e.get_Parameter(BuiltInParameter.DOOR_NUMBER);

            //using (Transaction trans = new Transaction(revitDoc, "Set Parameter"))
            //{
            //    trans.Start("Create Floors");
            //    Parameter PP_ = e.get_Parameter(BuiltInParameter.DOOR_NUMBER);
            //    PP_.Set("1234");
            //    trans.Commit();
            //}
            /////


            //FamilySymbol type_ = revitDoc.GetElement(e.GetTypeId()) as FamilySymbol;
            //ElementType type = revitDoc.GetElement(e.GetTypeId()) as ElementType;
            //Family fm = type_.Family;

            //var parameters = e.Parameters;
            //List<string> ParamLst = new List<string>();
            //List<string> StrlinLst = new List<string>();
            //List<BuiltInParameter> BPs = new List<BuiltInParameter>();
            //foreach (Parameter item in parameters)
            //{
            //    ParamLst.Add(item.Definition.Name);
            //    StrlinLst.Add(item.Definition.Name + " : "  + item.AsString() );
            //    InternalDefinition Int = item.Definition as InternalDefinition;
            //    BuiltInParameter BP = Int.BuiltInParameter;
            //    BPs.Add(BP);
            //    Parameter hh = type_.get_Parameter(BP);
            //}

            //BuiltInParameter height = BuiltInParameter.ALL_MODEL_TYPE_MARK;

            //Parameter h = type.get_Parameter(height);  
            //return Result.Succeeded;

            //CreateFloorByCADHashRegion test = new CreateFloorByCADHashRegion();
            //test.Main_Create(revitDoc, uidoc);

            MainForm mainform = new MainForm();
            mainform.ShowDialog();

            if (mainform.CASEName == 0)
            {
                CreateBeamsAndColumns Creation = new CreateBeamsAndColumns();
                Creation.Create_Version_2(revitDoc, uidoc);
            }
            else if (mainform.CASEName == 1)
            {
                Floor_Created_Main();

            }
            else if (mainform.CASEName == 2)
            {

                CreateLightObject Createion = new CreateLightObject();
                Createion.Main_Create(revitDoc, uidoc);
            }
            else
            {
                InsertComment ISC = new InsertComment();
                ISC.Main_Create(revitDoc, uidoc);

            }




            //GetCADTest(revitDoc);




            return Result.Succeeded;
        }

       





        /// <summary>
        /// Try To Get The Text From CAD But Failure
        /// </summary>
        /// <param name="revitDoc"></param>
        private void GetCADTest(Document revitDoc)
        {

            Document doc = uidoc.Document;





            View active_view = doc.ActiveView;
            List<GeometryObject> visible_dwg_geo = new List<GeometryObject>();

            // Pick Import Instance 
            Reference r = uidoc.Selection.PickObject(ObjectType.Element, new JtElementsOfClassSelectionFilter<ImportInstance>());
            ImportInstance dwg = doc.GetElement(r) as ImportInstance;
            FilteredElementCollector col  = new FilteredElementCollector(doc) .OfClass(typeof(ImportInstance));
             

            foreach (GeometryObject geometryObj in dwg.get_Geometry(new Options()))
            { 

                if (geometryObj is GeometryInstance) // This will be the whole thing
                {

                    GeometryInstance dwgInstance = geometryObj as GeometryInstance; 
                    var test = dwgInstance.Symbol;
                    foreach (GeometryObject blockObject in dwgInstance.SymbolGeometry)
                    {
                         
                        if (blockObject is GeometryInstance) // This could be a block
                        {
                            //get the object name and coordinates and rotation and 
                            //load into my own class 
                            GeometryInstance blockInstance = blockObject as GeometryInstance;

                            string name = blockInstance.Symbol.Name;

                            Transform transform = blockInstance.Transform;

                            XYZ origin = transform.Origin;

                            XYZ vectorTran = transform.OfVector(transform.BasisX.Normalize());
                            double rot = transform.BasisX.AngleOnPlaneTo(vectorTran, transform.BasisZ.Normalize()); // radians
                            rot = rot * (180 / Math.PI); // degrees
                             
                             
                        }
                    }
                }
            }


        }
        


        private void GetParameters(ParameterSet parameters)
        {

            List<string> ParamLst = new List<string>();
            List<string> StrlinLst = new List<string>();
            List<BuiltInParameter> BPs = new List<BuiltInParameter>();
            foreach (Parameter item in parameters)
            {
                ParamLst.Add(item.Definition.Name);
                StrlinLst.Add(item.Definition.Name + " : " + item.AsString());
                InternalDefinition Int = item.Definition as InternalDefinition;
                BuiltInParameter BP = Int.BuiltInParameter;
                BPs.Add(BP);
            }

        }


        private void Floor_Created_Main()
        {
            /// 取得所有樓層
            List<Level> levels = RevFind.GetLevels(revitDoc);
            /// 取得所有板的種類
            FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
            collector.OfCategory(BuiltInCategory.OST_Floors);
            List<Element> floorTypes = collector.ToList();

            /// 呼叫創立板之GUI
            Form2_Floor FormFloor = new Form2_Floor(levels, floorTypes);
            FormFloor.ShowDialog();

            if (FormFloor.DialogResult == System.Windows.Forms.DialogResult.OK)
            { 
                /// 取得梁目標樓層 
                Level targetLevelBeam = levels[FormFloor.cmbfloorLevel.SelectedIndex];
                /// 取得柱目標樓層 
                Level targetLevelCol = levels[FormFloor.cmbColLevel.SelectedIndex];

                /// 取得創立樓板種類
                FloorType floor_type = floorTypes[FormFloor.cmbFloorTypes.SelectedIndex] as FloorType;

                /// 創立建立樓板之物建
                CreateFloor_Version2 createFloors = new CreateFloor_Version2(revitDoc);

                /// 建立樓板
                createFloors.CreateFloor(targetLevelBeam, targetLevelCol, floor_type);


            }
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

