﻿using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Core
{
    public class GLTFPBR
    {
        public List<float> baseColorFactor { get; set; }

        public float metallicFactor { get; set; }

        public float roughnessFactor { get; set; }
    }
}
