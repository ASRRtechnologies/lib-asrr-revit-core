using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    internal class Collectors
    {
        public static Material GetRandomMaterial(Document doc)
        {
            using (var collector = new FilteredElementCollector(doc))
            {
                return collector.OfCategory(BuiltInCategory.OST_Materials)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Material>()
                    .FirstOrDefault();
            }
        }

        public static List<Element> AllVisibleElementsByView(Document doc, View view)
        {
            using (var collector = new FilteredElementCollector(doc, view.Id))
            {
                return collector.WhereElementIsNotElementType()
                    .ToElements()
                    .Where(e => e.CanBeHidden(view) && e.Category != null)
                    .ToList();
            }
        }

        public static List<Element> AllVisibleElementsByViewRfa(Document doc, View view)
        {
            using (var collector = new FilteredElementCollector(doc, view.Id))
            {
                return collector.WhereElementIsNotElementType()
                    .ToElements()
                    .Where(e => e.CanBeHidden(view))
                    .ToList();
            }
        }
    }
}