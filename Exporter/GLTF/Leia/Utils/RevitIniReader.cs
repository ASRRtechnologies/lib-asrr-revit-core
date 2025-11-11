using System.IO;
using System.Collections.Generic;
using System;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    public static class RevitIniReader
    {
        public static List<string> GetAdditionalRenderAppearancePaths()
        {
            // string revitVersion = ExternalApplication.RevitCollectorService.GetApplication().VersionNumber;
            var revitVersion = "2024"; // Placeholder for Revit version, replace with actual retrieval logic
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var iniDir = Path.Combine(
                appData,
                "Autodesk",
                "Revit",
                $"Autodesk Revit {revitVersion}"
            );

            var iniPath = Path.Combine(iniDir, "Revit.ini");

            if (!File.Exists(iniPath))
                return null;

            foreach (var line in File.ReadAllLines(iniPath))
                if (line.StartsWith("AdditionalRenderAppearancePaths="))
                {
                    var pathString = line.Substring("AdditionalRenderAppearancePaths=".Length);
                    var paths = pathString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    var absolutePaths = new List<string>();
                    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    foreach (var p in paths)
                    {
                        var trimmedPath = p.Trim();

                        // If already rooted, normalize it
                        if (Path.IsPathRooted(trimmedPath))
                        {
                            absolutePaths.Add(Path.GetFullPath(trimmedPath));
                        }
                        else
                        {
                            // Combine with user profile to resolve relative path
                            var fullPath = Path.GetFullPath(Path.Combine(userProfile, trimmedPath));
                            absolutePaths.Add(fullPath);
                        }
                    }

                    return absolutePaths;
                }

            return null;
        }
    }
}