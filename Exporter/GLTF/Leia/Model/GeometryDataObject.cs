using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    /// <summary>
    ///     Intermediate data format for converting between Revit Polymesh and glTF buffers.
    /// </summary>
    public class GeometryDataObject
    {
        public List<double> Vertices { get; set; } = new List<double>();

        public List<double> Normals { get; set; } = new List<double>();

        public List<UV> Uvs { get; set; } = new List<UV>();

        public List<int> Faces { get; set; } = new List<int>();
    }
}