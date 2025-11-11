using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    internal class GlbFile
    {
        public static void Create(Preferences preferences, List<GLTFBinaryData> binaryFileData, string json)
        {
            var jsonChunk = GlbJsonInfo.Get(json);
            var lenggg = jsonChunk.Length;
            var binChunk = GlbBinInfo.Get(binaryFileData, preferences);
            var headerChunk = GlbHeaderInfo.Get(jsonChunk, binChunk);

            var fileDirectory = string.Concat(preferences.path, ".glb");
            var exportArray = headerChunk.Concat(jsonChunk).Concat(binChunk).ToArray();

            File.WriteAllBytes(fileDirectory, exportArray);
        }
    }
}