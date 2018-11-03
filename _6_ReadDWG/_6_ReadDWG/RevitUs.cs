using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

namespace _6_ReadDWG
{  
    public class CreateObjects
    {
        public Document revitDoc;
        /// <summary>
        /// Constructor receive Document of RevitDoc
        /// </summary>
        /// <param name="_revitDoc"></param>
        public CreateObjects(Document _revitDoc)
        {
            this.revitDoc = _revitDoc;
        }

        /// <summary>
        /// Create Column 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="baseLevel"></param>
        /// <param name="topLevel"></param>
        /// <param name="points"></param>
        public void CreateColumn(FamilySymbol Type, Level baseLevel, Level topLevel, XYZ[] points)
        {

            if (!Type.IsActive)
            {
                using (Transaction trans = new Transaction(revitDoc))
                {
                    trans.Start("Activate Family instance");
                    Type.Activate();
                    trans.Commit();
                }

            }
            using (Transaction trans = new Transaction(revitDoc))
            {
                trans.Start("Create Column");
                FamilyInstance familyInstance = null;
                XYZ point = new XYZ((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2, 0);
                XYZ botPoint = new XYZ(point.X, point.Y, baseLevel.Elevation);
                XYZ topPoint = new XYZ(point.X, point.Y, topLevel.Elevation);
                familyInstance = revitDoc.Create.NewFamilyInstance(Line.CreateBound(botPoint, topPoint), Type, baseLevel, StructuralType.Column);
                trans.Commit();
            }
        }

        /// <summary>
        /// Create Beam
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="baseLevel"></param>
        /// <param name="points"></param>
        public void CreateBeam(FamilySymbol Type, Level baseLevel, XYZ[] points)
        {
            if (!Type.IsActive)
            {
                using (Transaction trans = new Transaction(revitDoc))
                {
                    trans.Start("Activate Family instance");
                    Type.Activate();
                    trans.Commit();
                }

            }

            using (Transaction trans = new Transaction(revitDoc))
            {

                trans.Start("Create Beam");
                FamilyInstance familyInstance = null;
                XYZ p1 = new XYZ(points[0].X, points[0].Y, baseLevel.Elevation);
                XYZ p2 = new XYZ(points[1].X, points[1].Y, baseLevel.Elevation);
                familyInstance = revitDoc.Create.NewFamilyInstance(Line.CreateBound(p1, p2), Type, baseLevel, StructuralType.Beam);
                trans.Commit();
            }
        }




    }

    public class FindRevitElements
    {  

        /// <summary>
        /// Get Beam Types
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Dictionary<string, List<FamilySymbol>> GetDocBeamTypes(Document doc)
        {
            return this.GetFamilyTypes(doc, BuiltInCategory.OST_StructuralFraming); 
        }

        /// <summary>
        /// Get Column Types
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Dictionary<string, List<FamilySymbol>> GetDocColumnsTypes(Document doc)
        {
            return this.GetFamilyTypes(doc, BuiltInCategory.OST_Columns);
        }

        /// <summary>
        /// Get Level
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public List<Level> GetLevels(Document document)
        {

            FilteredElementCollector viewCollector = new FilteredElementCollector(document);
            viewCollector.OfClass(typeof(Level));
            List<Level> ResLevel = new List<Level>();
            foreach (Element viewElement in viewCollector)
            {
                Level ll = (Level)viewElement;
                ResLevel.Add(ll);
            }
            return ResLevel;
        }
         
        /// <summary>
        /// Get Family Types
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        private Dictionary<string, List<FamilySymbol>> GetFamilyTypes(Document doc, BuiltInCategory cat)
        {
            return new FilteredElementCollector(doc)
                            .WherePasses(new ElementClassFilter(typeof(FamilySymbol)))
                            .WherePasses(new ElementCategoryFilter(cat))
                            .Cast<FamilySymbol>()
                            .GroupBy(e => e.Family.Name)
                            .ToDictionary(e => e.Key, e => e.ToList()); ;
        }
         
    }



}
