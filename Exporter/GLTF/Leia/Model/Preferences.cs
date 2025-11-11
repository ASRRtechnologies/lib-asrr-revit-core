using Autodesk.Revit.DB;
using System;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    public enum CompressionEnum
    {
        None,
        Meshopt,
        Draco,
        ZIP
    }

    public enum FormatEnum
    {
        gltf,
        glb
    }

    public enum MaterialsEnum
    {
        textures,
        materials,
        nonematerials
    }

    public class Preferences
    {
        public MaterialsEnum materials { get; set; } = MaterialsEnum.textures;

        public FormatEnum format { get; set; } = FormatEnum.glb;

        public bool normals { get; set; } = false;

        public bool levels { get; set; } = false;

        public bool grids { get; set; } = false;

        public bool batchId { get; set; } = false;

        public bool properties { get; set; } = false;

        public bool relocateTo0 { get; set; } = false;

        public bool flipAxis { get; set; } = true;

        public CompressionEnum compression { get; set; } = CompressionEnum.None;

#if REVIT2019 || REVIT2020
            public DisplayUnitType units { get; set; } = DisplayUnitType.DUT_METERS;

#else
        public ForgeTypeId units { get; set; } = UnitTypeId.Meters;

#endif

        public string path { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public string fileName { get; set; } = "test.glb";
    }
}