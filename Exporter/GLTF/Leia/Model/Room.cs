using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    public class Room
    {
        public string Name { get; set; }

        public Element Element { get; set; }

        public ElementId ElementId { get; set; }

        public List<ElementId> ElementList { get; set; }
    }
}