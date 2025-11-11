using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using System.Collections.Generic;
using Util = ASRR.Revit.Core.Exporter.GLTF.Leia.Utils.Util;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Transform
{
    public static class ModelScale
    {
        public static List<double> Get(Preferences preferences)
        {
            var scale = Util.ConvertFeetToUnitTypeId(preferences);
            return new List<double> { scale, scale, scale };
        }
    }
}