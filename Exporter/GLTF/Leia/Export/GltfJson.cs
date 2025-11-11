using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public static class GltfJson
    {
        public static string Get(
            List<GLTFScene> scenes,
            List<GLTFNode> nodes,
            List<GLTFMesh> meshes,
            List<GLTFMaterial> materials,
            List<GLTFBuffer> buffers,
            List<GLTFBufferView> bufferViews,
            List<GLTFAccessor> accessors,
            List<GLTFTexture> textures,
            List<GLTFImage> images,
            Preferences preferences)
        {
            var model = new Core.GLTF
    {
                asset = new GLTFVersion(),
                scenes = scenes,
                nodes = nodes,
                meshes = meshes
            };

            if (preferences.materials == MaterialsEnum.textures)
                model.extensionsUsed = new List<string> { "KHR_texture_transform" };

            if (materials.Any()) model.materials = materials;

            if (preferences.materials == MaterialsEnum.textures)
            {
                if (textures.Any()) model.textures = textures;

                if (images.Any()) model.images = images;
            }

            model.buffers = buffers;
            model.bufferViews = bufferViews;
            model.accessors = accessors;

            // Write the *.gltf file
            var serializedModel = JsonConvert.SerializeObject(
                model,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return serializedModel;
        }
    }
}