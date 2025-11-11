using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Core
{
    public class GLTFExtras
    {
        /// <summary>
        ///     Gets or sets the Revit created UniqueId for this object.
        /// </summary>
        public string uniqueId { get; set; }

        public RevitGridParametersObject gridParameters { get; set; }

        public Dictionary<string, string> parameters { get; set; }

        public long elementId { get; set; }

        public string elementCategory { get; set; }
    }
}