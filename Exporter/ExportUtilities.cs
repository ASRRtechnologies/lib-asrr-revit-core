﻿using System.Collections.Generic;
using System.IO;
using ASRR.Revit.Core.Utilities;
using Autodesk.Revit.DB;
using NLog;

namespace ASRR.Revit.Core.Exporter
{
    public static class ExportUtilities
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void ExportPNG(Document document, string exportPath)
        {
            var view = Collector.Get3dView(document);
            var opt = new ImageExportOptions
            {
                ZoomType = ZoomFitType.FitToPage,
                PixelSize = 600,
                FilePath = exportPath,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ShadowViewsFileType = ImageFileType.PNG,
                ImageResolution = ImageResolution.DPI_600,
                ExportRange = ExportRange.SetOfViews,
            };
            var ids = new List<ElementId> { view.Id };
            opt.SetViewsAndSheets(ids);
            document.ExportImage(opt);

            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }

            System.IO.File.Move(
                    exportPath.Replace(".png", "") + " - 3D View - {3D}.png",
                    exportPath);

            Log.Info($"Exporting PNG to: {exportPath}");
        }
    }
}