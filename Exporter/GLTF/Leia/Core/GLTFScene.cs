using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Core
{
    /// <summary>
    ///     The scenes available to render
    ///     https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#scenes.
    /// </summary>
    public class GLTFScene
    {
        public List<int> nodes = new List<int>();
    }
}