using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using Autodesk.Revit.DB;
using Material = Autodesk.Revit.DB.Material;


namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Materials
{
    public static class RevitMaterials
    {
        const int ONEINTVALUE = 1;

        /// <summary>
        /// Export Revit materials.
        /// </summary>
        public static GLTFMaterial Export(MaterialNode node,
            Preferences preferences, Document doc)
        {
            GLTFMaterial gl_mat = new GLTFMaterial();
            float opacity = ONEINTVALUE - (float)node.Transparency;

            Material material = doc.GetElement(node.MaterialId) as Material;

                if (material == null)
                {
                    return gl_mat;
                }

                gl_mat.name = material.Name;
                gl_mat.UniqueId = node.MaterialId.ToString();

                GLTFPBR pbr = new GLTFPBR();
                MaterialProperties.SetProperties(node, opacity, ref pbr, ref gl_mat);

                if (material != null && preferences.materials == MaterialsEnum.textures)
                {
                    MaterialTextures.SetMaterialTextures(material, gl_mat, doc, opacity);
                }

                MaterialProperties.SetMaterialColour(node, opacity, ref pbr, ref gl_mat);


            return gl_mat;
        }
    }
}

