using Util = ASRR.Revit.Core.Exporter.GLTF.Leia.Utils.Util;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using System;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Utils;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Transform
{
    public static class ModelTraslation
    {
        public static List<float> GetPointToRelocate(Document doc, View view, double scale, Preferences preferences)
        {
            if (preferences.relocateTo0)
            {
                if (doc == null)
                    throw new ArgumentNullException(nameof(doc), "Document was null in GetPointToRelocate");

                if (preferences == null)
                    throw new ArgumentNullException(nameof(preferences), "Preferences was null in GetPointToRelocate");
            
                var elementsOnActiveView = new List<Element>();
                if (doc.IsFamilyDocument)
                    elementsOnActiveView = Collectors.AllVisibleElementsByViewRfa(doc, view);
                else
                    elementsOnActiveView = Collectors.AllVisibleElementsByView(doc, view);

                var bb = Util.GetElementsBoundingBox(view, elementsOnActiveView);

                if (preferences.flipAxis)
                {
                    var pointX = -scale * ((bb.Min.X + bb.Max.X) / 2);
                    var pointy = -scale * bb.Min.Z;
                    var pointz = -scale * ((bb.Min.Y + bb.Max.Y) / 2);
                    return new List<float> { (float)pointX, (float)pointy, -(float)pointz };
                }
                else
                {
                    var pointX = -scale * ((bb.Min.X + bb.Max.X) / 2);
                    var pointy = -scale * ((bb.Min.Z + bb.Max.Z) / 2);
                    var pointz = -scale * bb.Min.Y;
                    return new List<float> { (float)pointX, (float)pointz, (float)pointy };
                }
            }

            return new List<float> { 0, 0, 0 };
        }
    }
}