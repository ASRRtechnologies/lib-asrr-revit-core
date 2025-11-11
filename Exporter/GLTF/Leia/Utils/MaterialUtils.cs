using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    public class MaterialUtils
    {
        public static Material GetMeshMaterial(Document doc, Mesh mesh)
        {
            var materialId = mesh.MaterialElementId;

            if (materialId != null) return doc.GetElement(materialId) as Material;

            return null;
        }

        public static GLTFMaterial GetGltfMeshMaterial(Document doc, Preferences preferences, Mesh mesh,
            IndexedDictionary<GLTFMaterial> materials, bool doubleSided)
        {
            var gl_mat = new GLTFMaterial();

            var material = GetMeshMaterial(doc, mesh);

            if (preferences.materials == MaterialsEnum.materials || preferences.materials == MaterialsEnum.textures)
            {
                if (material == null)
                {
                    gl_mat = GLTFExportUtils.GetGLTFMaterial(materials, 1, doubleSided);
                }
                else
                {
                    gl_mat.doubleSided = doubleSided;
                    var opacity = 1 - (float)material.Transparency;
                    gl_mat.name = material.Name;
                    var pbr = new GLTFPBR();
                    pbr.baseColorFactor = new List<float>(4)
                        { material.Color.Red / 255f, material.Color.Green / 255f, material.Color.Blue / 255f, opacity };
                    pbr.metallicFactor = 0f;
                    pbr.roughnessFactor = 1f;
                    gl_mat.pbrMetallicRoughness = pbr;
                    gl_mat.UniqueId = material.UniqueId;
                }
            }

            return gl_mat;
        }
    }
}