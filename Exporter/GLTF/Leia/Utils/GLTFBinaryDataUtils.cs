using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Materials;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    public class GLTFBinaryDataUtils
    {
        private const string SCALAR_STR = "SCALAR";
        private const string FACE_STR = "FACE";
        private const string BATCH_ID_STR = "BATCH_ID";

        private const string VEC3_STR = "VEC3";
        private const string POSITION_STR = "POSITION";

        private const string NORMAL_STR = "NORMALS";

        public static int ExportFaces(int bufferIdx, int byteOffset, GeometryDataObject geomData, GLTFBinaryData bufferData,
            List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors)
        {
            foreach (var index in geomData.Faces) bufferData.indexBuffer.Add(index);

            // Get max and min for index data
            var faceMinMax = Util.GetScalarMinMax(bufferData.indexBuffer);

            // Add a faces / indexes buffer view
            var elementsPerIndex = 1;
            var bytesPerIndexElement = 4;
            var bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            var numIndexes = geomData.Faces.Count;
            var sizeOfIndexView = numIndexes * bytesPerIndex;
            var facesView = new GLTFBufferView(bufferIdx, byteOffset, sizeOfIndexView, Targets.ELEMENT_ARRAY_BUFFER,
                string.Empty);
            bufferViews.Add(facesView);
            var facesViewIdx = bufferViews.Count - 1;

            // add a face accessor
            var count = geomData.Faces.Count / elementsPerIndex;
            var max = new List<float>(1) { faceMinMax[1] };
            var min = new List<float>(1) { faceMinMax[0] };
            var faceAccessor = new GLTFAccessor(facesViewIdx, 0, ComponentType.UNSIGNED_INT, count, SCALAR_STR, max, min,
                FACE_STR);
            accessors.Add(faceAccessor);
            bufferData.indexAccessorIndex = accessors.Count - 1;
            return byteOffset + facesView.byteLength;
        }

        public static int ExportVertices(int bufferIdx, int byteOffset, GeometryDataObject geomData,
            GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors,
            out int sizeOfVec3View, out int elementsPerVertex)
        {
            for (var i = 0; i < geomData.Vertices.Count; i++)
                bufferData.vertexBuffer.Add(Convert.ToSingle(geomData.Vertices[i]));

            // Get max and min for vertex data
            var vertexMinMax = Util.GetVec3MinMax(bufferData.vertexBuffer);

            // Add a vec3 buffer view
            elementsPerVertex = 3;
            var bytesPerElement = 4;
            var bytesPerVertex = elementsPerVertex * bytesPerElement;
            var numVec3 = geomData.Vertices.Count / elementsPerVertex;
            sizeOfVec3View = numVec3 * bytesPerVertex;

            var vec3View = new GLTFBufferView(bufferIdx, byteOffset, sizeOfVec3View, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(vec3View);
            var vec3ViewIdx = bufferViews.Count - 1;

            // add a position accessor
            var count = geomData.Vertices.Count / elementsPerVertex;
            var max = new List<float>(3) { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
            var min = new List<float>(3) { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };

            var positionAccessor =
                new GLTFAccessor(vec3ViewIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, POSITION_STR);
            accessors.Add(positionAccessor);
            bufferData.vertexAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3View.byteLength;
        }

        public static int ExportNormals(int bufferIdx, int byteOffset, GeometryDataObject geomData,
            GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors)
        {
            for (var i = 0; i < geomData.Normals.Count; i++)
                bufferData.normalBuffer.Add(Convert.ToSingle(geomData.Normals[i]));

            // Get max and min for normal data
            var normalMinMax = Util.GetVec3MinMax(bufferData.normalBuffer);

            // Add a normals (vec3) buffer view
            var elementsPerNormal = 3;
            var bytesPerNormalElement = 4;
            var bytesPerNormal = elementsPerNormal * bytesPerNormalElement;
            var normalsCount = geomData.Normals.Count;
            var numVec3Normals = normalsCount / elementsPerNormal;
            var sizeOfVec3ViewNormals = numVec3Normals * bytesPerNormal;
            var vec3ViewNormals = new GLTFBufferView(bufferIdx, byteOffset, sizeOfVec3ViewNormals, Targets.ARRAY_BUFFER,
                string.Empty);
            bufferViews.Add(vec3ViewNormals);
            var vec3ViewNormalsIdx = bufferViews.Count - 1;

            // add a normals accessor
            var count = normalsCount / elementsPerNormal;
            var max = new List<float>(3) { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
            var min = new List<float>(3) { normalMinMax[0], normalMinMax[2], normalMinMax[4] };

            var normalsAccessor = new GLTFAccessor(vec3ViewNormalsIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min,
                NORMAL_STR);
            accessors.Add(normalsAccessor);
            bufferData.normalsAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3ViewNormals.byteLength;
        }

        public static int ExportBatchId(int bufferIdx, int byteOffset, int sizeOfVec3View, int elementsPerVertex,
            long elementId, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews,
            List<GLTFAccessor> accessors)
        {
            for (var i = 0; i < geomData.Vertices.Count; i++) bufferData.batchIdBuffer.Add(elementId);

            // Get max and min for batchId data
            var batchIdMinMax = Util.GetVec3MinMax(bufferData.batchIdBuffer);

            // Add a batchId buffer view
            var batchIdsView =
                new GLTFBufferView(bufferIdx, byteOffset, sizeOfVec3View, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(batchIdsView);
            var batchIdsViewIdx = bufferViews.Count - 1;

            // add a batchId accessor
            var count = geomData.Vertices.Count / elementsPerVertex;
            var max = new List<float>(3) { batchIdMinMax[1], batchIdMinMax[3], batchIdMinMax[5] };
            var min = new List<float>(3) { batchIdMinMax[0], batchIdMinMax[2], batchIdMinMax[4] };
            var batchIdAccessor = new GLTFAccessor(batchIdsViewIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min,
                BATCH_ID_STR);
            accessors.Add(batchIdAccessor);
            bufferData.batchIdAccessorIndex = accessors.Count - 1;
            return byteOffset + batchIdsView.byteLength;
        }

        public static int ExportUVs(
            int bufferIdx,
            int byteOffset,
            GeometryDataObject geomData,
            GLTFBinaryData bufferData,
            List<GLTFBufferView> bufferViews,
            List<GLTFAccessor> accessors)
        {
            const string VEC2_STR = "VEC2";
            const string TEXCOORD_STR = "TEXCOORD_0";

            var uvCount = geomData.Uvs.Count;

            // Convert UVs to float buffer (U, V per entry)
            foreach (var uv in geomData.Uvs)
            {
                bufferData.uvBuffer.Add((float)uv.U);
                bufferData.uvBuffer.Add((float)uv.V);
            }

            var elementsPerUV = 2;
            var bytesPerUVElement = 4;
            var bytesPerUV = elementsPerUV * bytesPerUVElement;


            var sizeOfUVView = uvCount * bytesPerUV;

            // Create UV buffer view
            var uvBufferView = new GLTFBufferView(bufferIdx, byteOffset, sizeOfUVView, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(uvBufferView);
            var uvBufferViewIdx = bufferViews.Count - 1;

            // Min/max for VEC2 accessors (optional, but good practice)
            var uvMinMax = Util.GetVec2MinMax(bufferData.uvBuffer);
            var max = new List<float> { uvMinMax[1], uvMinMax[3] };
            var min = new List<float> { uvMinMax[0], uvMinMax[2] };

            // Create UV accessor
            var uvAccessor = new GLTFAccessor(
                uvBufferViewIdx,
                0,
                ComponentType.FLOAT,
                uvCount,
                VEC2_STR,
                max,
                min,
                TEXCOORD_STR);

            accessors.Add(uvAccessor);
            bufferData.uvAccessorIndex = accessors.Count - 1;

            return byteOffset + uvBufferView.byteLength;
        }

        public static int ExportImageBuffer(
            int bufferIdx,
            int byteOffset,
            GLTFMaterial material,
            List<GLTFImage> images,
            List<GLTFTexture> textures,
            GLTFBinaryData bufferData,
            List<GLTFBufferView> bufferViews)
        {
            if (material.EmbeddedTexturePath == null) return byteOffset;

            if (material.pbrMetallicRoughness.baseColorTexture.index == -1)
            {
                var imageBytes = File.ReadAllBytes(material.EmbeddedTexturePath);
                (string, ImageFormat) mimeType = BitmapsUtils.GetMimeType(material.EmbeddedTexturePath);

                byte[] blendedBytes = BitmapsUtils.BlendImageWithColor(imageBytes, material.Fadevalue,
                    material.BaseColor, mimeType.Item2, material.TintColour);

                if (blendedBytes != null)
                {
                    if (bufferData.byteData == null)
                    {
                        bufferData.byteData = blendedBytes;
                    }
                    else
                    {
                        var combined = new byte[bufferData.byteData.Length + blendedBytes.Length];
                        Buffer.BlockCopy(bufferData.byteData, 0, combined, 0, bufferData.byteData.Length);
                        Buffer.BlockCopy(blendedBytes, 0, combined, bufferData.byteData.Length, blendedBytes.Length);
                        bufferData.byteData = combined;
                    }
                }

                var currentLenght = blendedBytes.Length;
                var alignment = 4;
                var padding = (alignment - currentLenght % alignment) % alignment;

                if (padding != 0)
                {
                    currentLenght = currentLenght + padding;

                    var newArray = bufferData.byteData.Concat(new byte[padding]).ToArray();
                    bufferData.byteData = newArray;
                }

                var ImageBufferView = new GLTFBufferView(bufferIdx, byteOffset, currentLenght, Targets.NONE, string.Empty);

                bufferViews.Add(ImageBufferView);
                var bufferViewIndex = bufferViews.Count - 1;

                var image = new GLTFImage
                {
                    bufferView = bufferViewIndex,
                    mimeType = mimeType.Item1
                };

                images.Add(image);
                var imageIndex = images.Count - 1;

                var texture = new GLTFTexture
                {
                    source = imageIndex
                };
                textures.Add(texture);
                var textureIndex = textures.Count - 1;
                material.pbrMetallicRoughness.baseColorTexture.index = textureIndex;
                return byteOffset + ImageBufferView.byteLength;
            }

            return byteOffset;
        }
    }
}