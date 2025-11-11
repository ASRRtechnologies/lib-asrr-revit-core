using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public static class GlbBinInfo
    {
        public static byte[] Get(List<GLTFBinaryData> binaryFileData, Preferences preferences)
        {
            var binData = new List<byte>();

            foreach (var bin in binaryFileData)
            {
                foreach (var coord in bin.vertexBuffer)
                {
                    var vertex = BitConverter.GetBytes(coord).ToList();
                    binData.AddRange(vertex);
                }

                if (preferences.normals)
                    foreach (var normal in bin.normalBuffer)
                    {
                        var normalBuffer = BitConverter.GetBytes(normal).ToList();
                        binData.AddRange(normalBuffer);
                    }

                if (preferences.materials == MaterialsEnum.textures)
                {
                    if (bin.byteData != null) binData.AddRange(bin.byteData);

                    if (bin.uvBuffer != null && bin.uvBuffer.Count > 0)
                        foreach (var uv in bin.uvBuffer)
                        {
                            var uvBytes = BitConverter.GetBytes(uv).ToList();
                            binData.AddRange(uvBytes);
                        }
                }

                if (preferences.batchId)
                    foreach (var batchId in bin.batchIdBuffer)
                    {
                        var batchIdBuffer = BitConverter.GetBytes(batchId).ToList();
                        binData.AddRange(batchIdBuffer);
                    }

                foreach (var index in bin.indexBuffer)
                {
                    var indexIdBuffer = BitConverter.GetBytes(index).ToList();
                    binData.AddRange(indexIdBuffer);
                }
            }

            if (binData.Count % 4 != 0)
            {
                var missingNumbers = 4 - binData.Count % 4;
                for (var i = 0; i < missingNumbers; i++)
                {
                    var emptyByte = (byte)00;
                    var zeros = new List<byte> { emptyByte };
                    binData.AddRange(zeros);
                }
            }

            var glbBin = new GlbBin();
            glbBin.ChunkData = binData.ToArray();
            glbBin.Length = BitConverter.GetBytes(Convert.ToUInt32(glbBin.ChunkData.Length));

            var result = new byte[] { };
            result = result.Concat(glbBin.Length).ToArray();
            result = result.Concat(glbBin.ChunkType()).ToArray();
            result = result.Concat(glbBin.ChunkData).ToArray();

            return result;
        }
    }
}