using ASRR.Revit.Core.Utilities;
using Autodesk.Revit.DB;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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


            using (var transaction = new Transaction(document))
            {
                transaction.Start("Set view graphics to Shaded");
                view.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE).Set(3); //3 = Shaded
                transaction.Commit();
            }

            document.ExportImage(opt);

            if (File.Exists(exportPath))
                File.Delete(exportPath);

            var exportDir = new FileInfo(exportPath).DirectoryName;
            var exportedPNG = Directory.GetFiles(exportDir).First();
            File.Move(exportedPNG,
                exportPath);

            Log.Debug($"Exporting PNG to: {exportPath}");
        }
    }
}