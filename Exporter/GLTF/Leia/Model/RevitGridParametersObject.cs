using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    public class RevitGridParametersObject
    {
        public List<double> origin { get; set; }

        public List<double> direction { get; set; }

        public double length { get; set; }
    }
}