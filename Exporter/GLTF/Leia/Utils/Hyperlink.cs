using System;
using System.Diagnostics;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    public static class Hyperlink
    {
        public static void Run(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}