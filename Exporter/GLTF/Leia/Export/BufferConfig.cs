using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public static class BufferConfig
    {
        private const string BIN = ".bin";

        public static void Run(List<GLTFBufferView> bufferViews, List<GLTFBuffer> buffers,
            Preferences preferences)
        {
            var bytePosition = 0;
            var currentBuffer = 0;

            foreach (var view in bufferViews)
            {
                if (view.buffer.Equals(0))
                {
                    bytePosition += view.byteLength;
                    continue;
                }

                if (view.buffer != currentBuffer)
                {
                    view.buffer = 0;
                    view.byteOffset = bytePosition;
                    bytePosition += view.byteLength;
                }
            }

            var buffer = new GLTFBuffer();

            if (preferences.format == FormatEnum.gltf)
            {
                var bufferUri = string.Concat(preferences.fileName, BIN);
                buffer.uri = bufferUri;
            }

            buffer.byteLength = bytePosition;
            buffers.Clear();
            buffers.Add(buffer);
        }
    }
}