namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Core
{
    /// <summary>
    ///     Properties defining where the GPU should look to find the mesh and material data
    ///     https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#meshes.
    /// </summary>
    public class GLTFMeshPrimitive
    {
        public GLTFAttribute attributes { get; set; } = new GLTFAttribute();

        public int indices { get; set; }

        public int? material { get; set; } = null;

        public int mode { get; set; } = 4; // 4 is triangles
    }
}