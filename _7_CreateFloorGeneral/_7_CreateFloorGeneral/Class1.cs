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

namespace _7_CreateFloorGeneral
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

            /// 取得CAD所有線條資訊
            CADOperation CADGet = new CADOperation();
            Dictionary<string, List<Geometry>> CADRes = CADGet.GeneralCAD(uidoc);
            /// 取得所有樓板類型
            List<Element> floorTypes = RevFind.GetFloorTypes(revitDoc);
            /// 取得所有牆類型 
            List<Element> wallTypes = RevFind.GetWallTypes(revitDoc);
            /// 取得所有Level
            List<Level> levels = RevFind.GetLevels(revitDoc);


            Form1 Form = new Form1(CADRes.Keys.ToList(), floorTypes, wallTypes, levels);
            Form.ShowDialog();

            int CADTarget = Form.cmbCADLayers.SelectedIndex == 0 ? -1 : Form.cmbCADLayers.SelectedIndex;
            Level targetLevel = levels[Form.cmbLevels.SelectedIndex];

            List<string> CADKeys = Form.cmbCADLayers.SelectedIndex == 0 ? CADRes.Keys.ToList() :
                                                                       new List<string>() { Form.cmbCADLayers.Text };

            if (Form.radFloor.Checked)
            {
                FloorType floor_type = floorTypes[Form.cmbTypes.SelectedIndex] as FloorType;
                foreach (string key in CADKeys)
                {
                    List<Geometry> lines = CADRes[key];
                    List<ClosedCurve> closedCurves = GetClosedCurves(lines);
                    foreach (ClosedCurve item in closedCurves)
                    {
                        if (item.IsClosed())
                        {
                            CreateFloors(item.Data, floor_type, targetLevel);
                        }
                    }
                }
            }
            else
            {
                WallType wall_type = wallTypes[Form.cmbTypes.SelectedIndex] as WallType;
                foreach (string key in CADKeys)
                {
                    List<Geometry> lines = CADRes[key];
                    List<ClosedCurve> closedCurves = GetClosedCurves(lines);
                    foreach (ClosedCurve item in closedCurves)
                    {
                        if (Form.cmbCurvesType.SelectedIndex == 0)
                        {
                            if (item.IsClosed())
                            {
                                CreateWalls(item.Data, wall_type, targetLevel, Form.txtHeigth.Text);
                            }
                        }
                        else if (Form.cmbCurvesType.SelectedIndex == 1)
                        {
                            if (!item.IsClosed())
                            {
                                CreateWalls(item.Data, wall_type, targetLevel, Form.txtHeigth.Text);
                            }
                        }
                        else
                        { 
                            CreateWalls(item.Data, wall_type, targetLevel, Form.txtHeigth.Text);

                        }
                    }

                }
            }

            //List<Geometry> lines = CADRes["0"];
            //List<ClosedCurve> closedCurves = GetClosedCurves(lines);



            //List<Geometry> lines = CADRes[Form.cmbCADLayers.Text];
            //List<ClosedCurve> closedCurves = GetClosedCurves(lines);
            //foreach (ClosedCurve item in closedCurves)
            //{
            //    if (item.IsClosed())
            //    {
            //        CreateFloors(item.Data, floor_type, targetLevel);
            //    }
            //}



            return Result.Succeeded;
        }



        private void CreateFloors(List<Geometry> closedCurves, FloorType floor_type, Level targetLevel)
        {

            CurveArray curveArray = new CurveArray();
            Curve Cr = null;
            foreach (Geometry item in closedCurves)
            {
                if (item.Type == "Line")
                {
                    curveArray.Append(Line.CreateBound(item.startPoint, item.endPoint));
                }
                else
                {

                    Cr = Arc.Create(item.startPoint, item.endPoint, item.ThirdPoint);
                    curveArray.Append(Cr);
                }

            }

            using (Transaction transaction = new Transaction(revitDoc))
            {
                transaction.Start("Start ");
                revitDoc.Create.NewFloor(curveArray, floor_type, targetLevel, false);
                transaction.Commit();

            }
        }


        private void CreateWalls(List<Geometry> closedCurves, WallType Wall_type, Level targetLevel, string Height_Wall)
        {

            CurveArray curveArray = new CurveArray();
            Curve Cr = null;
            foreach (Geometry item in closedCurves)
            {
                if (item.Type == "Line")
                {
                    curveArray.Append(Line.CreateBound(item.startPoint, item.endPoint));
                }
                else
                {

                    Cr = Arc.Create(item.startPoint, item.endPoint, item.ThirdPoint);
                    curveArray.Append(Cr);
                }

            }

            double Height_ = Convert.ToDouble(Height_Wall);
            double Height = UnitUtils.Convert(Height_, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
            using (Transaction transaction = new Transaction(revitDoc))
            {
                transaction.Start("Start ");
                foreach (Curve item in curveArray)
                {
                    //Wall wall = Wall.Create(revitDoc, item, targetLevel.Id, false);
                    //wall.WallType = Wall_type;
                    Wall wall = Wall.Create(revitDoc, item, Wall_type.Id, targetLevel.Id, Height, 0, false, false);

                }
                transaction.Commit();

            }
        }






        /// <summary>
        /// 取得所有連續曲線
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private List<ClosedCurve> GetClosedCurves(List<Geometry> lines)
        {
            double ACCURENCY = 0.00001;
            List<ClosedCurve> ClosedCureves = new List<ClosedCurve>();
            int[] picked = new int[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                int j = 0;
                if (!lines[i].IsBound) continue;

                ClosedCurve closedCurce = new ClosedCurve();
                closedCurce.Add(lines[i]);
                while (j < lines.Count)
                {
                    if (j != i && picked[j] == 0)
                    {
                        if (lines[j].IsBound && (lines[j].startPoint - closedCurce.GetLastItem().endPoint).GetLength() < ACCURENCY)
                        {
                            closedCurce.Add(lines[j]);
                            picked[j] = -1;
                            j = 0;
                        }
                        else if (lines[j].IsBound && (lines[j].endPoint - closedCurce.GetLastItem().endPoint).GetLength() < ACCURENCY)
                        {
                            if (lines[j].Type == "Line")
                            {
                                LINE newLine = new LINE(lines[j].endPoint, lines[j].startPoint);
                                newLine.Type = "Line";
                                closedCurce.Add(newLine);
                                picked[j] = -1;
                                j = 0;
                            }
                            else
                            {
                                Geometry newArc = lines[j];
                                newArc.Reverse();
                                closedCurce.Add(newArc);
                                picked[j] = -1;
                                j = 0;
                            }
                        }
                    }
                    j++;
                }

                if (closedCurce.GetCount() > 1)
                {

                    picked[i] = -1;
                    ClosedCureves.Add(closedCurce);
                }
            }

            return ClosedCureves;
        }
    }


}
