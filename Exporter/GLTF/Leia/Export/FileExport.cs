using System.Collections.Generic;
using System.IO;
using System.Text;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public static class FileExport
    {
        private const string BIN = ".bin";
        private const string GLTF = ".gltf";

        public static void Run(
            Preferences preferences,
            List<GLTFBufferView> bufferViews,
            List<GLTFBuffer> buffers,
            List<GLTFBinaryData> binaryFileData, List<GLTFScene> scenes,
            IndexedDictionary<GLTFNode> nodes,
            IndexedDictionary<GLTFMesh> meshes,
            IndexedDictionary<GLTFMaterial> materials,
            List<GLTFAccessor> accessors,
            List<GLTFTexture> textures,
            List<GLTFImage> images)
        {
            if (preferences.format == FormatEnum.gltf)
            {
                BufferConfig.Run(bufferViews, buffers, preferences);
                var fileDirectory = string.Concat(preferences.path, BIN);
                BinFile.Create(fileDirectory, binaryFileData, preferences);

                var gltfJson = GltfJson.Get(scenes, nodes.List, meshes.List, materials.List, buffers,
                    bufferViews, accessors, textures, images, preferences);

                var gltfName = string.Concat(preferences.path, GLTF);
                var utf8WithoutBom = new UTF8Encoding(false);
                File.WriteAllText(gltfName, gltfJson, utf8WithoutBom);
            }
            else
            {
                BufferConfig.Run(bufferViews, buffers, preferences);

                var gltfJson = GltfJson.Get(scenes, nodes.List, meshes.List, materials.List, buffers,
                    bufferViews, accessors, textures, images, preferences);

                GlbFile.Create(preferences, binaryFileData, gltfJson);
            }
        }
    }
}