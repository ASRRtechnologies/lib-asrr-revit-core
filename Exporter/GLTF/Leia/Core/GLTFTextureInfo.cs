using Newtonsoft.Json;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Core
{
    /// <summary>
    ///     Texture information for a material property
    ///     https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#texture-info
    /// </summary>
    public class GLTFTexture
    {
        public int source { get; set; }
    }

    public class GLTFTextureInfo
    {
        public int index { get; set; }
        public int texCoord { get; set; } = 0;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GLTFTextureExtensions extensions { get; set; }
    }

    public class GLTFTextureTransform
    {
        public float[] offset { get; set; } = new[] { 0.0f, 0.0f };
        public float[] scale { get; set; } = new[] { 1.0f, 1.0f };
        public float rotation { get; set; } = 0.0f;
        public int texCoord { get; set; } = 0;
    }

    public class GLTFTextureExtensions
    {
        [JsonProperty("KHR_texture_transform")]
        public GLTFTextureTransform TextureTransform { get; set; }
    }
}