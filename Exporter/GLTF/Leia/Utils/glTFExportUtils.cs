
using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    public class GLTFExportUtils
    {
        private const int DEF_COLOR = 250;
        private const string DEF_MATERIAL_NAME = "default";
        private const string DEF_UNIQUEL_ID = "8a3c94b3-d9e2-4e57-9189-f9bb6a9a54a4";

        private const string UNDERSCORE = "_";

        private const string BIN = ".bin";

        public static GLTFMaterial GetGLTFMaterial(IndexedDictionary<GLTFMaterial> gltfMaterials, double opacity,
            bool doubleSided)
        {
            if (gltfMaterials.Dict.ContainsKey(DEF_UNIQUEL_ID)) return gltfMaterials.GetElement(DEF_UNIQUEL_ID);

            return CreateDefaultGLTFMaterial((int)opacity, doubleSided);
        }

        public static GLTFMaterial CreateDefaultGLTFMaterial(int materialOpacity, bool doubleSided)
        {
            var gl_mat = new GLTFMaterial();
            gl_mat.doubleSided = doubleSided;
            var opacity = 1 - (float)materialOpacity;
            gl_mat.name = DEF_MATERIAL_NAME;
            var pbr = new GLTFPBR();
            pbr.baseColorFactor = new List<float>(4) { 1f, 1f, 1f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = 1f;
            gl_mat.pbrMetallicRoughness = pbr;
            gl_mat.UniqueId = DEF_UNIQUEL_ID;

            return gl_mat;
        }

        public static void AddVerticesAndFaces(
            VertexLookupIntObject vertexLookup,
            GeometryDataObject geometryDataObject,
            List<XYZ> pts)
        {
            foreach (var pt in pts)
            {
                var point = new PointIntObject(pt);
                var index = vertexLookup.AddVertexAndFlatten(point, geometryDataObject.Vertices);
                geometryDataObject.Faces.Add(index);
            }
        }

        public static void AddOrUpdateCurrentItem(
            Element element,
            IndexedDictionary<GeometryDataObject> geomDataObj,
            IndexedDictionary<VertexLookupIntObject> vertexIntObj,
            GLTFMaterial material)
        {
            // Add new "_current" entries if vertex_key is unique
            var vertex_key = string.Concat(element.UniqueId, UNDERSCORE, material.UniqueId);
            geomDataObj.AddOrUpdateCurrent(vertex_key, new GeometryDataObject());
            vertexIntObj.AddOrUpdateCurrent(vertex_key, new VertexLookupIntObject());
        }

        public static void AddRPCNormals(Preferences preferences, MeshTriangle triangle, GeometryDataObject geomDataObj)
        {
            var normal = GeometryUtils.GetNormal(triangle);

            for (var j = 0; j < 3; j++)
            {
                geomDataObj.Normals.Add(normal.X);
                geomDataObj.Normals.Add(normal.Y);
                geomDataObj.Normals.Add(normal.Z);
            }
        }

        /// <summary>
        ///     Takes the intermediate geometry data and performs the calculations
        ///     to convert that into glTF buffers, views, and accessors.
        /// </summary>
        /// <param name="buffers">buffers.</param>
        /// <param name="accessors">accessors.</param>
        /// <param name="bufferViews">bufferViews.</param>
        /// <param name="geomData">geomData.</param>
        /// <param name="name">Unique name for the .bin file that will be produced.</param>
        /// <param name="elementId">Revit element's Element ID that will be used as the batchId value.</param>
        /// <param name="exportBatchId">exportBatchId.</param>
        /// <param name="exportNormals">exportNormals.</param>
        /// <returns>Returns the GLTFBinaryData object.</returns>
        public static GLTFBinaryData AddGeometryMeta(
            List<GLTFBuffer> buffers,
            List<GLTFAccessor> accessors,
            List<GLTFBufferView> bufferViews,
            GeometryDataObject geomData,
            string name,
            long elementId,
            Preferences preferences,
            GLTFMaterial material,
            List<GLTFImage> images,
            List<GLTFTexture> textures)
        {
            var byteOffset = 0;

            // add a buffer
            var buffer = new GLTFBuffer();
            buffer.uri = string.Concat(name, BIN);
            buffers.Add(buffer);
            var bufferIdx = buffers.Count - 1;
            var bufferData = new GLTFBinaryData();
            bufferData.name = buffer.uri;


            byteOffset = GLTFBinaryDataUtils.ExportVertices(bufferIdx, byteOffset, geomData, bufferData, bufferViews,
                accessors, out var sizeOfVec3View, out var elementsPerVertex);

            if (preferences.normals)
                byteOffset =
                    GLTFBinaryDataUtils.ExportNormals(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors);

            if (preferences.materials == MaterialsEnum.textures &&
                material.pbrMetallicRoughness?.baseColorTexture != null && geomData.Uvs.Count != 0)
            {
                byteOffset = GLTFBinaryDataUtils.ExportImageBuffer(bufferIdx, byteOffset, material, images, textures,
                    bufferData, bufferViews);
                byteOffset =
                    GLTFBinaryDataUtils.ExportUVs(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors);
            }

            if (preferences.batchId)
                byteOffset = GLTFBinaryDataUtils.ExportBatchId(bufferIdx, byteOffset, sizeOfVec3View, elementsPerVertex,
                    elementId, geomData, bufferData, bufferViews, accessors);

            byteOffset =
                GLTFBinaryDataUtils.ExportFaces(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors);

            buffers[bufferIdx].byteLength = byteOffset;

            return bufferData;
        }


        public static void AddNormals(Autodesk.Revit.DB.Transform transform, PolymeshTopology polymesh,
            List<double> normals)
        {
            IList<XYZ> polymeshNormals = polymesh.GetNormals();

            switch (polymesh.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                {
                    foreach (var facet in polymesh.GetFacets())
                    {
                        var normalPoints = new List<XYZ>
                        {
                            transform.OfVector(polymeshNormals[facet.V1]).Normalize(),
                            transform.OfVector(polymeshNormals[facet.V2]).Normalize(),
                            transform.OfVector(polymeshNormals[facet.V3]).Normalize()
                        };

                        foreach (var normalPoint in normalPoints)
                        {
                            normals.Add(normalPoint.X);
                            normals.Add(normalPoint.Y);
                            normals.Add(normalPoint.Z);
                        }
                    }

                    break;
                }

                case DistributionOfNormals.OnePerFace:
                {
                    foreach (var facet in polymesh.GetFacets())
                    foreach (var normal in polymesh.GetNormals())
                    {
                        var newNormal = transform.OfVector(normal).Normalize();

                        for (var j = 0; j < 3; j++)
                        {
                            normals.Add(newNormal.X);
                            normals.Add(newNormal.Y);
                            normals.Add(newNormal.Z);
                        }
                    }

                    break;
                }

                case DistributionOfNormals.OnEachFacet:
                {
                    foreach (var normal in polymeshNormals)
                    {
                        var newNormal = transform.OfVector(normal).Normalize();

                        normals.Add(newNormal.X);
                        normals.Add(newNormal.Y);
                        normals.Add(newNormal.Z);
                    }

                    break;
                }
            }
        }
    }
}