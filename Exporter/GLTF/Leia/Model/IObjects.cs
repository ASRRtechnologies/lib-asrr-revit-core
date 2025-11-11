using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    public interface IObjects<T>
    {
        List<T> ObjectsList { get; set; }

        int Count { get; set; }

        Category Category { get; set; }
    }
}