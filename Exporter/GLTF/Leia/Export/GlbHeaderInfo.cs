using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using System;
using System.Linq;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public static class GlbHeaderInfo
    {
        public static byte[] Get(byte[] json, byte[] bin)
        {
            var glbHeader = new GlbHeader();
            glbHeader.Length = BitConverter.GetBytes(Convert.ToUInt32(json.Length + bin.Length + 12));

            var result = new byte[] { };
            result = result.Concat(glbHeader.Magic()).ToArray();
            result = result.Concat(glbHeader.Version()).ToArray();
            result = result.Concat(glbHeader.Length).ToArray();

            return result;
        }
    }
}