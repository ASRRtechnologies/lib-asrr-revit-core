using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Export
{
    public class Compression
    {
        /// <summary>
        ///     Run compression.
        /// </summary>
        /// <param name="preferences">preferences.</param>
        public static void Run(Preferences preferences)
        {
            switch (preferences.compression)
            {
                case CompressionEnum.ZIP:
                    // progressBar.Message = "Compressing to ZIP";
                    ZIP.Compress(preferences);
                    break;
                case CompressionEnum.Draco:
                    // progressBar.Message = "Compressing to Draco";
                    Draco.Compress(preferences);
                    break;
                case CompressionEnum.Meshopt:
                    // progressBar.Message = "Compressing to MeshOpt";
                    MeshOpt.Compress(preferences);
                    break;
            }
        }
    }
}