using System;
using System.Linq;
using System.Text;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public static class GlbJsonInfo
    {
        public static byte[] Get(string json)
        {
            var glbJson = new GlbJson();

            // Convert JSON to UTF-8 byte array (DO THIS FIRST)
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            // Calculate padding needed to align to 4 bytes
            var padding = (4 - jsonBytes.Length % 4) % 4;

            // Allocate new array for padded JSON
            var paddedJsonBytes = new byte[jsonBytes.Length + padding];
            Array.Copy(jsonBytes, paddedJsonBytes, jsonBytes.Length);

            // Pad with spaces (0x20)
            for (var i = jsonBytes.Length; i < paddedJsonBytes.Length; i++) paddedJsonBytes[i] = 0x20;

            glbJson.ChunkData = paddedJsonBytes;

            // Write chunk length (uint32)
            glbJson.Length = BitConverter.GetBytes(Convert.ToUInt32(glbJson.ChunkData.Length));

            // Build JSON chunk: length + type + data
            var result = new byte[] { };
            result = result.Concat(glbJson.Length).ToArray();
            result = result.Concat(glbJson.ChunkType()).ToArray(); // Should return 4-byte ASCII for "JSON"
            result = result.Concat(glbJson.ChunkData).ToArray();

            return result;
        }
    }
}