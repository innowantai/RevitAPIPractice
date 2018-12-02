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


namespace _7_CreateFloorGeneral
{
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


        public List<Element> GetFloorTypes(Document revitDoc)
        {
            /// 取得所有板的種類
            FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
            collector.OfCategory(BuiltInCategory.OST_Floors);
            //List<Element> floorTypes = collector.ToList();

            return collector.ToList();
        }


        public List<Element> GetWallTypes(Document revitDoc)
        {
            /// 取得所有板的種類
            FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
            collector.OfCategory(BuiltInCategory.OST_Walls);
            //List<Element> floorTypes = collector.ToList();

            return collector.ToList();
        }
    }
}
