using System.Collections.Generic;
using System.IO;
using System.Text;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public static class BinFile
    {
        /// <summary>
        ///     Create a new .bin file.
        /// </summary>
        /// <param name="filename">.bin file name.</param>
        /// <param name="binaryFileData">binary file data.</param>
        /// <param name="exportNormals">export normals.</param>
        /// <param name="exportBatchId">export BatchId.</param>
        public static void Create(string filename, List<GLTFBinaryData> binaryFileData,
            Preferences preferences)
        {
            using (var f = File.Create(filename))
            using (var writer = new BinaryWriter(new BufferedStream(f), Encoding.UTF8))
            {
                foreach (var bin in binaryFileData)
                {
                    for (var i = 0; i < bin.vertexBuffer.Count; i++) writer.Write(bin.vertexBuffer[i]);

                    if (preferences.normals)
                        for (var i = 0; i < bin.normalBuffer.Count; i++)
                            writer.Write(bin.normalBuffer[i]);

                    if (preferences.materials == MaterialsEnum.textures)
                    {
                        if (bin.byteData != null) writer.Write((byte[])bin.byteData);

                        if (bin.uvBuffer != null && bin.uvBuffer.Count > 0)
                            for (var i = 0; i < bin.uvBuffer.Count; i++)
                                writer.Write(bin.uvBuffer[i]);
                    }

                    if (preferences.batchId)
                        for (var i = 0; i < bin.batchIdBuffer.Count; i++)
                            writer.Write(bin.batchIdBuffer[i]);

                    for (var i = 0; i < bin.indexBuffer.Count; i++) writer.Write(bin.indexBuffer[i]);
                }

                writer.Flush();
            }
        }
    }
}