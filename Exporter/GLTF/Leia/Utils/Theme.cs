using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.UI;
using Color = System.Windows.Media.Color;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    public static class Theme
    {
        public static void ApplyDarkLightMode(ResourceDictionary resourceDictionary)
        {
            // Get the current theme
            var currentTheme = UIThemeManager.CurrentTheme;

            // Check if the theme is Dark or Light
            if (currentTheme == UITheme.Dark) ApplyDarkMode(resourceDictionary);
        }

        private static void ApplyDarkMode(ResourceDictionary resourceDictionary)
        {
            var colorMappings = new Dictionary<string, string>
            {
                { "BackgroundColor", "#18263c" },
                { "MainGray", "#ffffff" },
                { "SecondaryGray", "#ffffff" },
                { "HoverGray", "#465162" },
                { "WhiteColour", "#18263c" },
                { "AuxiliaryGray", "#667075" },
                { "SecondaryBackgroundColor", "#515a6c" }
            };

            foreach (var entry in colorMappings)
                resourceDictionary[entry.Key] = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(entry.Value));
        }
    }
}