using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace _3_CreateBeam
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {

        public Application revitApp;
        public Document revitDoc;
        public UIDocument uidoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

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
             

            FloorType FT = new FilteredElementCollector(revitDoc)
                                   .OfClass(typeof(FloorType))
                                   .First<Element>(
                                     e => e.Name.Equals("160mm 混凝土與 50mm 金屬板"))
                                     as FloorType;


            Transaction transaction = new Transaction(revitDoc);
            FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
            FailureHandler failureHandler = new FailureHandler();
            failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
            failureHandlingOptions.SetClearAfterRollback(false);
            transaction.SetFailureHandlingOptions(failureHandlingOptions);
            transaction.Start("Transaction Name");
            // Do something here that causes the error

            List<string> Height = new List<string>();
            List<XYZ> Data = LoadTest(ref Height, @"C:\Users\Wantai\Desktop\CADAPI\CombineTreeAndCircle\CombineTreeAndCircle\bin\Debug\final.txt");

            int kk = 0;
            foreach (var xyz in Data)
            {
                CreateFloor(xyz.X, xyz.Y, double.Parse(Height[kk]), xyz.Z, FT, ele);
                kk = kk + 1;
            }
            transaction.Commit();





            // The following is just illustrative. 
            // In reality we would collect the errors 
            // to show later.

            //if (failureHandler.ErrorMessage != "")
            //{
            //    System.Windows.Forms.MessageBox.Show(
            //      failureHandler.ErrorSeverity + " || "
            //      + failureHandler.ErrorMessage);
            //}

            return Result.Succeeded;
        }

        void CreateFloor(double x, double y, double hh, double rr, FloorType FT,ElementId ele)
        {
            double nx = UnitUtils.ConvertToInternalUnits(x, DisplayUnitType.DUT_MILLIMETERS);
            double ny = UnitUtils.ConvertToInternalUnits(y, DisplayUnitType.DUT_MILLIMETERS);
            double nh = UnitUtils.ConvertToInternalUnits(hh, DisplayUnitType.DUT_MILLIMETERS);
            double nrr = UnitUtils.ConvertToInternalUnits(rr, DisplayUnitType.DUT_MILLIMETERS); 
            CurveArray cur = new CurveArray();
            Arc arc1 = Arc.Create(Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), new XYZ(nx, ny, nh)), nrr, 0, Math.PI);
            Arc arc2 = Arc.Create(Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), new XYZ(nx, ny, nh)), nrr, Math.PI, Math.PI * 2);
            cur.Append(arc1);
            cur.Append(arc2); 
            Units u = new Units(UnitSystem.Metric);
            revitDoc.SetUnits(u);

            Floor floor = revitDoc.Create.NewFloor(cur, FT, revitDoc.GetElement(ele) as Level, false);
            //Floor floor = revitDoc.Create.NewFloor(cur, false);
        }


        void CreateFloorOri(double x, double y, double hh, double rr)
        { 
            double nx = UnitUtils.ConvertToInternalUnits(x, DisplayUnitType.DUT_MILLIMETERS);
            double ny = UnitUtils.ConvertToInternalUnits(y, DisplayUnitType.DUT_MILLIMETERS);
            double nh = UnitUtils.ConvertToInternalUnits(hh, DisplayUnitType.DUT_MILLIMETERS);
            double nrr = UnitUtils.ConvertToInternalUnits(rr, DisplayUnitType.DUT_MILLIMETERS);
            CurveArray cur = new CurveArray();
            Arc arc1 = Arc.Create(Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), new XYZ(nx, ny, hh)), nrr, 0, Math.PI);
            Arc arc2 = Arc.Create(Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), new XYZ(nx, ny, hh)), nrr, Math.PI, Math.PI * 2);
            cur.Append(arc1);
            cur.Append(arc2);
            using (Transaction tran = new Transaction(revitDoc))
            {
                tran.Start("Create floor : ");
                Units u = new Units(UnitSystem.Metric);
                revitDoc.SetUnits(u);
                 Floor floor = revitDoc.Create.NewFloor(cur, false);
                //Floor floor = revitDoc.Create.NewFloor(cur,;
                tran.Commit();
            }
        }
        List<XYZ> LoadTest(ref List<string> Height, string path)
        {
            StreamReader sr = new StreamReader(path);
            List<XYZ> points = new List<XYZ>(); 
            while (sr.Peek() != -1)
            {
                string[] tmp = sr.ReadLine().Split(',');
                points.Add(new XYZ(double.Parse(tmp[0]), double.Parse(tmp[1]), double.Parse(tmp[2])));
                Height.Add(tmp[3]);
            }
            sr.Close();
            return points;

        }

         


        /// <summary>
        /// tmp
        /// </summary>
        /// <param name="commandData"></param>
        void changeSlop(ExternalCommandData commandData)
        { 
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            Reference ref1 = sel.PickObject(
              ObjectType.Element, "Please pick a floor.");

            Floor f = doc.GetElement(ref1.ElementId) as Floor;

            if (f != null)
            { 

                // Retrieve floor edge model line elements.

                ICollection<ElementId> deleted_ids;

                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Temporarily Delete Floor");

                    deleted_ids = doc.Delete(f.Id);

                    tx.RollBack();
                }

                // Grab the first floor edge model line.

                ModelLine ml = null;

                foreach (ElementId id in deleted_ids)
                {
                    ml = doc.GetElement(id) as ModelLine;

                    if (null != ml)
                    {
                        break;
                    }
                }

                if (null != ml)
                {
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Change Slope Angle");

                        // This parameter is read only. Therefore,
                        // the change does not work and we cannot 
                        // change the floor slope angle after the 
                        // floor is created. 
                        ml.get_Parameter(
                          BuiltInParameter.CURVE_IS_SLOPE_DEFINING)
                            .Set(1);

                        ml.get_Parameter(
                          BuiltInParameter.ROOF_SLOPE)
                            .Set(1.2);

                        tx.Commit();
                    }
                }
            }
        }




















        public class FailureHandler : IFailuresPreprocessor
        {
            public string ErrorMessage { set; get; }
            public string ErrorSeverity { set; get; }

            public FailureHandler()
            {
                ErrorMessage = "";
                ErrorSeverity = "";
            }

            public FailureProcessingResult PreprocessFailures(
              FailuresAccessor failuresAccessor)
            {
                IList<FailureMessageAccessor> failureMessages
                  = failuresAccessor.GetFailureMessages();

                foreach (FailureMessageAccessor
                  failureMessageAccessor in failureMessages)
                {
                    // We're just deleting all of the warning level 
                    // failures and rolling back any others

                    FailureDefinitionId id = failureMessageAccessor
                      .GetFailureDefinitionId();

                    try
                    {
                        ErrorMessage = failureMessageAccessor
                          .GetDescriptionText();
                    }
                    catch
                    {
                        ErrorMessage = "Unknown Error";
                    }

                    try
                    {
                        FailureSeverity failureSeverity
                          = failureMessageAccessor.GetSeverity();

                        ErrorSeverity = failureSeverity.ToString();

                        if (failureSeverity == FailureSeverity.Warning)
                        {
                            failuresAccessor.DeleteWarning(
                              failureMessageAccessor);
                        }
                        else
                        {
                            return FailureProcessingResult
                              .ProceedWithRollBack;
                        }
                    }
                    catch
                    {
                    }
                }
                return FailureProcessingResult.Continue;
            }
        }









        //void Delete(ExternalCommandData commandData)
        //{
        //    UIApplication app = commandData.Application;
        //    Document doc = app.ActiveUIDocument.Document;
        //    app.Application.FailuresProcessing += new EventHandler<Autodesk.Revit.DB.Events.FailuresProcessingEventArgs>(Application_FailuresProcessing);
        //}
        //private void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        //{
        //    //Get failuresAccessor
        //    FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
        //    String transactionName = failuresAccessor.GetTransactionName();

        //    IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
        //    if (fmas.Count == 0)
        //    {
        //        // FailureProcessingResult.Continue is to let the failure cycle continue next step.
        //        e.SetProcessingResult(FailureProcessingResult.Continue);
        //        return;
        //    }
        //    //If manually delete a element, the transaction name is “Delete Selection”
        //    //If the failure is caused by deleting element.
        //    if (transactionName.Equals("Delete Selection"))
        //    {
        //        foreach (FailureMessageAccessor fma in fmas)
        //        {
        //            //Below line mimic clicking "Remove Link" button by resolving the failure.             
        //            failuresAccessor.ResolveFailure(fma);

        //            //Below line mimic clicking "Ok" button by just deleting the warning.
        //            //failuresAccessor.DeleteWarning(fma);         
        //        }
        //        e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
        //        return;
        //    }
        //    e.SetProcessingResult(FailureProcessingResult.Continue);
        //}

















































        //void CreateWallType_1(ElementId levelId)
        //{
        //    using (Transaction tran = new Transaction(revitDoc))
        //    {
        //        tran.Start("Create Wall");
        //        Wall wall = Wall.Create(revitDoc, Line.CreateBound(new XYZ(0, 0, 0), new XYZ(100, 0, 0)), levelId, false);
        //        tran.Commit();
        //    }
        //} 
        //void CreateWallType_2()
        //{
        //    IList<Curve> curves = new List<Curve>();
        //    curves.Add(Line.CreateBound(new XYZ(100, 20, 0), new XYZ(100, -20, 0)));
        //    curves.Add(Line.CreateBound(new XYZ(100, -20, 0), new XYZ(100, -10, 10)));
        //    curves.Add(Line.CreateBound(new XYZ(100, -10, 10), new XYZ(100, 10, 10)));
        //    curves.Add(Line.CreateBound(new XYZ(100, 10, 10), new XYZ(100, 20, 0)));
        //    using (Transaction trans = new Transaction(revitDoc))
        //    {
        //        trans.Start("Create Wall");
        //        Wall wall = Wall.Create(revitDoc, curves, false);  /// The third parameter is to indicate that wall is the Structural or not 
        //        trans.Commit();

        //    }
        //} 
        //void CreateWallType_4(ElementId eleId, ElementId levelId)
        //{
        //    Wall wall = revitDoc.GetElement(eleId) as Wall;
        //    ElementId wallTypeId = wall.GetTypeId();
        //    XYZ[] vertexes = new XYZ[] { new XYZ(0, 0, 0), new XYZ(0, 100, 0), new XYZ(0, 0, 100) };
        //    IList<Curve> curves = new List<Curve>();

        //    for (int i = 0; i < vertexes.Length; i++)
        //    {
        //        if (i != vertexes.Length - 1)
        //        {
        //            curves.Add(Line.CreateBound(vertexes[i], vertexes[i + 1]));
        //        }
        //        else
        //        {
        //            curves.Add(Line.CreateBound(vertexes[i], vertexes[0]));
        //        }
        //    }

        //    using (Transaction trans = new Transaction(revitDoc))
        //    {
        //        trans.Start("Create wall 1");
        //        wall = Wall.Create(revitDoc, curves, wallTypeId, levelId, false, new XYZ(-1, 0, 0));
        //        trans.Commit();
        //    }

        //    curves.Clear();
        //    vertexes = new XYZ[] { new XYZ(0, 0, 100), new XYZ(0, 100, 100), new XYZ(0, 100, 0) };

        //    for (int i = 0; i < vertexes.Length; i++)
        //    {
        //        if (i != vertexes.Length - 1)
        //        {
        //            curves.Add(Line.CreateBound(vertexes[i], vertexes[i + 1]));
        //        }
        //        else
        //        {
        //            curves.Add(Line.CreateBound(vertexes[i], vertexes[0]));
        //        }
        //    }

        //    using (Transaction trans = new Transaction(revitDoc))
        //    {
        //        trans.Start("Create wall 1");
        //        wall = Wall.Create(revitDoc, curves, wallTypeId, levelId, false, new XYZ(1, 0, 0));
        //        trans.Commit();
        //    }





        //} 
        ///// <summary>
        ///// Page 84, example 4-5 Code
        ///// </summary>
        ///// <param name="eleId"></param>
        //void GetWallInformation(ElementId eleId)
        //{

        //    Wall wall = revitDoc.GetElement(eleId) as Wall;
        //    CompoundStructure compoundStructure = wall.WallType.GetCompoundStructure();
        //    foreach (CompoundStructureLayer cc in compoundStructure.GetLayers())
        //    {
        //        ElementId eleID = cc.MaterialId;
        //        double layerWidth = UnitUtils.ConvertToInternalUnits(cc.Width, DisplayUnitType.DUT_MILLIMETERS);
        //        TaskDialog.Show("rev", "ID = " + eleID.ToString() + " ,Width = " + layerWidth.ToString());
        //    }
        //}  
        ///// <summary>
        ///// Page 82, example 4-2 ~ 4-3
        ///// </summary>
        //void CreateNewLevel()
        //{
        //    Level level = null;
        //    using (Transaction trans = new Transaction(revitDoc))
        //    {
        //        trans.Start("Create New Level");
        //        level = Level.Create(revitDoc, UnitUtils.ConvertToInternalUnits(1000, DisplayUnitType.DUT_MILLIMETERS));
        //        trans.Commit();
        //    }

        //    var classFilter = new ElementClassFilter(typeof(ViewFamilyType));
        //    FilteredElementCollector filterElements = new FilteredElementCollector(revitDoc);
        //    filterElements = filterElements.WherePasses(classFilter);
        //    foreach (ViewFamilyType viewFamilyType in filterElements)
        //    {
        //        if (viewFamilyType.ViewFamily == ViewFamily.FloorPlan || viewFamilyType.ViewFamily == ViewFamily.CeilingPlan)
        //        {
        //            using (Transaction trans = new Transaction(revitDoc))
        //            {
        //                try
        //                {
        //                    trans.Start("CreateView");
        //                    ViewPlan view = ViewPlan.Create(revitDoc, viewFamilyType.Id, level.Id);
        //                    trans.Commit();

        //                }
        //                catch (Exception e)
        //                {

        //                    TaskDialog.Show("ee", e.Message);
        //                }
        //            }
        //        }
        //    }

        //}

        ///// <summary>
        ///// Below code will be delete 
        ///// </summary>
        //void CreateWall()
        //{
        //    IList<Curve> curves = new List<Curve>();
        //    curves.Add(Line.CreateBound(new XYZ(100, 20, 0), new XYZ(100, -20, 0)));
        //    curves.Add(Line.CreateBound(new XYZ(100, -20, 0), new XYZ(100, -10, 10)));
        //    curves.Add(Line.CreateBound(new XYZ(100, -10, 10), new XYZ(100, 10, 10)));
        //    curves.Add(Line.CreateBound(new XYZ(100, 10, 10), new XYZ(100, 20, 0)));
        //    using (Transaction tran = new Transaction(revitDoc))
        //    {
        //        tran.Start("Create Wall");
        //        Wall wall = Wall.Create(revitDoc, curves, false);
        //        tran.Commit();
        //    }
        //}

        //void CreateDoorsInWall(Autodesk.Revit.DB.Document document, Wall wall)
        //{
        //    // get wall's level for door creation
        //    Level level = document.GetElement(wall.LevelId) as Level;

        //    FilteredElementCollector collector = new FilteredElementCollector(document);
        //    ICollection<Element> collection = collector.OfClass(typeof(FamilySymbol))
        //                                                .OfCategory(BuiltInCategory.OST_Doors)
        //                                                .ToElements();
        //    IEnumerator<Element> symbolItor = collection.GetEnumerator();

        //    double x = 0, y = 0, z = 0;
        //    while (symbolItor.MoveNext())
        //    {
        //        FamilySymbol symbol = symbolItor.Current as FamilySymbol;
        //        XYZ location = new XYZ(x, y, z);
        //        FamilyInstance instance = document.Create.NewFamilyInstance(location, symbol, wall, level, StructuralType.NonStructural);
        //        x += 10;
        //        y += 10;
        //        z += 1.5;
        //    }
        //}




    } 
}
