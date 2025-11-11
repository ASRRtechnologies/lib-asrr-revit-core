using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    public class FixedObjects
    {
        public FixedObjects()
        {
            ObjectsList = new List<FixedObject>();
        }

        public List<FixedObject> ObjectsList { get; set; }

        public int Count
        {
            get => Count;
            set => Count = ObjectsList.Count();
        }

        public Category Category
        {
            get => Category;
            set => Category = ObjectsList.FirstOrDefault().Category;
        }
    }
}