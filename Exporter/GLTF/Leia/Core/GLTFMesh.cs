using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Core
{
    /// <summary>
    ///     The array of primitives defining the mesh of an object
    ///     https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#meshes.
    /// </summary>
    public class GLTFMesh
    {
        public List<GLTFMeshPrimitive> primitives { get; set; }

        public string name { get; set; }
    }
}