using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Core
{
    public class GLTFPBR
    {
        public List<float> baseColorFactor { get; set; }

        public float metallicFactor { get; set; }

        public float roughnessFactor { get; set; }

        // Texture properties
        public GLTFTextureInfo baseColorTexture { get; set; }
        public GLTFTexture metallicRoughnessTexture { get; set; }
    }
}