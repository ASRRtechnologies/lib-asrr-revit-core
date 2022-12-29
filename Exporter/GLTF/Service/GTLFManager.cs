﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using ASRR.Revit.Core.Exporter.GLTF.Model;
using Autodesk.Revit.DB;

namespace glTFRevitExport
{
    internal static class ManagerUtils
    {
        public static List<double> ConvertXForm(Transform xform)
        {
            if (xform == null || xform.IsIdentity) return null;

            var BasisX = xform.BasisX;
            var BasisY = xform.BasisY;
            var BasisZ = xform.BasisZ;
            var Origin = xform.Origin;
            var OriginX = PointInt.ConvertFeetToMillimetres(Origin.X);
            var OriginY = PointInt.ConvertFeetToMillimetres(Origin.Y);
            var OriginZ = PointInt.ConvertFeetToMillimetres(Origin.Z);

            var glXform = new List<double>(16)
            {
                BasisX.X, BasisX.Y, BasisX.Z, 0,
                BasisY.X, BasisY.Y, BasisY.Z, 0,
                BasisZ.X, BasisZ.Y, BasisZ.Z, 0,
                OriginX, OriginY, OriginZ, 1
            };

            return glXform;
        }

        public static string GenerateSHA256Hash<T>(T data)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, data);

            using (var hasher = SHA256.Create())
            {
                mStream.Position = 0;
                var byteHash = hasher.ComputeHash(mStream);

                var sBuilder = new StringBuilder();
                for (var i = 0; i < byteHash.Length; i++) sBuilder.Append(byteHash[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }

        public class HashSearch
        {
            private readonly string _S;

            public HashSearch(string s)
            {
                _S = s;
            }

            public bool EqualTo(HashedType d)
            {
                return d.hashcode.Equals(_S);
            }
        }
    }

    internal class GLTFManager
    {
        /// <summary>
        ///     Toggles the export of JSON properties as a glTF Extras
        ///     object on each node.
        /// </summary>
        private bool _exportProperties = true;

        /// <summary>
        ///     Flag to write coords as Z up instead of Y up (if true).
        ///     CAUTION: With local coordinate systems and transforms, this no longer
        ///     produces expected results. TODO on fixing it, however there is a larger
        ///     philisophical debtate to be had over whether flipping coordinates in
        ///     source CAD applications should EVER be the correct thing to do (as opposed to
        ///     flipping the camera in the viewer).
        /// </summary>
        private readonly bool _flipCoords = false;

        /// <summary>
        ///     List of all Accessors referencing the BufferViews.
        /// </summary>
        public List<glTFAccessor> accessors = new List<glTFAccessor>();

        /// <summary>
        ///     Container for the vertex/face/normal information
        ///     that will be serialized into a binary format
        ///     for the final *.bin files.
        /// </summary>
        public List<glTFBinaryData> binaryFileData = new List<glTFBinaryData>();

        /// <summary>
        ///     List of all buffers referencing the binary file data.
        /// </summary>
        public List<glTFBuffer> buffers = new List<glTFBuffer>();

        /// <summary>
        ///     List of all BufferViews referencing the buffers.
        /// </summary>
        public List<glTFBufferView> bufferViews = new List<glTFBufferView>();

        /// <summary>
        ///     Stack maintaining the geometry containers for each
        ///     node down the current scene graph branch. These are popped
        ///     as we retreat back up the graph.
        /// </summary>
        private readonly Stack<Dictionary<string, GeometryData>> geometryStack =
            new Stack<Dictionary<string, GeometryData>>();

        /// <summary>
        ///     Stateful, uuid indexable list of all materials in the export.
        /// </summary>
        private readonly IndexedDictionary<glTFMaterial> materialDict = new IndexedDictionary<glTFMaterial>();

        /// <summary>
        ///     Hashable container for mesh data, to aid instancing.
        /// </summary>
        private readonly List<MeshContainer> meshContainers = new List<MeshContainer>();

        /// <summary>
        ///     Dictionary of nodes keyed to their unique id.
        /// </summary>
        private readonly Dictionary<string, Node> nodeDict = new Dictionary<string, Node>();

        /// <summary>
        ///     Stack maintaining the uniqueId's of each node down
        ///     the current scene graph branch.
        /// </summary>
        private readonly Stack<string> parentStack = new Stack<string>();

        /// <summary>
        ///     List of root nodes defining scenes.
        /// </summary>
        public List<glTFScene> scenes = new List<glTFScene>();

        /// <summary>
        ///     Ordered list of all nodes
        /// </summary>
        public List<glTFNode> nodes
        {
            get
            {
                var list = nodeDict.Values.ToList();
                return list.OrderBy(x => x.index).Select(x => x.ToGLTFNode()).ToList();
            }
        }

        /// <summary>
        ///     List of all materials referenced by meshes.
        /// </summary>
        public List<glTFMaterial> materials => materialDict.List;

        /// <summary>
        ///     List of all meshes referenced by nodes.
        /// </summary>
        public List<glTFMesh> meshes
        {
            get { return meshContainers.Select(x => x.contents).ToList(); }
        }

        /// <summary>
        ///     The uniqueId of the currently open node.
        /// </summary>
        private string currentNodeId => parentStack.Peek();

        /// <summary>
        ///     The geometry container for the currently open node.
        /// </summary>
        private Dictionary<string, GeometryData> currentGeom => geometryStack.Peek();

        /// <summary>
        ///     Returns proper tab alignment for displaying element
        ///     hierarchy in debug printing.
        /// </summary>
        public string formatDebugHeirarchy
        {
            get
            {
                var spaces = "";
                for (var i = 0; i < parentStack.Count; i++) spaces += "  ";

                return spaces;
            }
        }

        /// <summary>
        ///     Returns true if the unique id is already present in the list of nodes.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public bool containsNode(string uniqueId)
        {
            return nodeDict.ContainsKey(uniqueId);
        }

        public void Start(bool exportProperties = true)
        {
            _exportProperties = exportProperties;

            var rootNode = new Node(0);
            rootNode.children = new List<int>();
            nodeDict.Add(rootNode.id, rootNode);
            parentStack.Push(rootNode.id);

            var defaultScene = new glTFScene();
            defaultScene.nodes.Add(0);
            scenes.Add(defaultScene);
        }

        public glTFContainer Finish()
        {
            var model = new glTF();
            model.asset = new glTFVersion();
            model.scenes = scenes;
            model.nodes = nodes;
            model.meshes = meshes;
            model.materials = materials;
            model.buffers = buffers;
            model.bufferViews = bufferViews;
            model.accessors = accessors;

            var container = new glTFContainer();
            container.glTF = model;
            container.binaries = binaryFileData;

            return container;
        }

        public void OpenNode(Element elem, Transform xform = null, bool isInstance = false)
        {
            //// TODO: [RM] Commented out because this is likely to be very buggy and not the 
            //// correct solution intent is to prevent creation of new nodes when a symbol 
            //// is a child of an instance of the same type.
            //// Witness: parking spaces and stair railings for examples of two
            //// different issues with the behavior
            //if (isInstance == true && elem is FamilySymbol)
            //{
            //    FamilyInstance parentInstance = nodeDict[currentNodeId].element as FamilyInstance;
            //    if (
            //        parentInstance != null &&
            //        parentInstance.Symbol != null &&
            //        elem.Name == parentInstance.Symbol.Name
            //    )
            //    {
            //        nodeDict[currentNodeId].matrix = ManagerUtils.ConvertXForm(xform);
            //        return;
            //    }

            //    //nodeDict[currentNodeId].matrix = ManagerUtils.ConvertXForm(xform);
            //    //return;
            //}
            var exportNodeProperties = _exportProperties;
            if (isInstance && elem is FamilySymbol) exportNodeProperties = false;

            var node = new Node(elem, nodeDict.Count, exportNodeProperties, isInstance, formatDebugHeirarchy);

            if (parentStack.Count > 0)
            {
                var parentId = parentStack.Peek();
                var parentNode = nodeDict[parentId];
                if (parentNode.children == null) parentNode.children = new List<int>();
                nodeDict[parentId].children.Add(node.index);
            }

            parentStack.Push(node.id);
            if (xform != null) node.matrix = ManagerUtils.ConvertXForm(xform);

            nodeDict.Add(node.id, node);

            OpenGeometry();
            Debug.WriteLine(string.Format("{0}Node Open", formatDebugHeirarchy));
        }

        public void CloseNode(Element elem = null, bool isInstance = false)
        {
            //// TODO: [RM] Commented out because this is likely to be very buggy and not the 
            //// correct solution intent is to prevent creation of new nodes when a symbol 
            //// is a child of an instance of the same type.
            //// Witness: parking spaces and stair railings for examples of two
            //// different issues with the behavior
            //if (isInstance && elem is FamilySymbol)
            //{
            //    FamilyInstance parentInstance = nodeDict[currentNodeId].element as FamilyInstance;
            //    if (
            //        parentInstance != null &&
            //        parentInstance.Symbol != null &&
            //        elem.Name == parentInstance.Symbol.Name
            //    )
            //    {
            //        return;
            //    }
            //    //return;
            //}

            Debug.WriteLine(string.Format("{0}Closing Node", formatDebugHeirarchy));

            if (currentGeom != null) CloseGeometry();

            Debug.WriteLine(string.Format("{0}  Node Closed", formatDebugHeirarchy));
            parentStack.Pop();
        }

        public void SwitchMaterial(MaterialNode matNode, string name = null, string id = null)
        {
            var gl_mat = new glTFMaterial();
            gl_mat.name = name;

            var pbr = new glTFPBR();
            pbr.baseColorFactor = new List<float>
            {
                matNode.Color.Red / 255f,
                matNode.Color.Green / 255f,
                matNode.Color.Blue / 255f,
                1f - (float) matNode.Transparency
            };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            materialDict.AddOrUpdateCurrent(id, gl_mat);
        }

        public void OpenGeometry()
        {
            geometryStack.Push(new Dictionary<string, GeometryData>());
        }

        public void OnGeometry(PolymeshTopology polymesh)
        {
            if (currentNodeId == null) throw new Exception();

            var vertex_key = currentNodeId + "_" + materialDict.CurrentKey;
            if (currentGeom.ContainsKey(vertex_key) == false) currentGeom.Add(vertex_key, new GeometryData());

            // Populate normals from this polymesh
            var norms = polymesh.GetNormals();
            foreach (var norm in norms)
            {
                currentGeom[vertex_key].normals.Add(norm.X);
                currentGeom[vertex_key].normals.Add(norm.Y);
                currentGeom[vertex_key].normals.Add(norm.Z);
            }

            // Populate vertex and faces data
            var pts = polymesh.GetPoints();
            foreach (var facet in polymesh.GetFacets())
            {
                var v1 = currentGeom[vertex_key].vertDictionary.AddVertex(new PointInt(pts[facet.V1], _flipCoords));
                var v2 = currentGeom[vertex_key].vertDictionary.AddVertex(new PointInt(pts[facet.V2], _flipCoords));
                var v3 = currentGeom[vertex_key].vertDictionary.AddVertex(new PointInt(pts[facet.V3], _flipCoords));

                currentGeom[vertex_key].faces.Add(v1);
                currentGeom[vertex_key].faces.Add(v2);
                currentGeom[vertex_key].faces.Add(v3);
            }
        }

        public void CloseGeometry()
        {
            Debug.WriteLine(string.Format("{0}  Closing Geometry", formatDebugHeirarchy));
            // Create the new mesh and populate the primitives with GeometryData
            var mesh = new glTFMesh();
            mesh.primitives = new List<glTFMeshPrimitive>();

            // transfer ordered vertices from vertex dictionary to vertices list
            foreach (var key_geom in currentGeom)
            {
                var key = key_geom.Key;
                var geom = key_geom.Value;
                foreach (var point_index in geom.vertDictionary)
                {
                    var point = point_index.Key;
                    geom.vertices.Add(point.X);
                    geom.vertices.Add(point.Y);
                    geom.vertices.Add(point.Z);
                }

                // convert GeometryData objects into glTFMeshPrimitive
                var material_key = key.Split('_')[1];

                var bufferMeta = processGeometry(geom, key);
                if (bufferMeta.hashcode != null) binaryFileData.Add(bufferMeta);

                var primative = new glTFMeshPrimitive();

                primative.attributes.POSITION = bufferMeta.vertexAccessorIndex;
                primative.indices = bufferMeta.indexAccessorIndex;
                primative.material = materialDict.GetIndexFromUUID(material_key);
                // TODO: Add normal attribute accessor index here

                mesh.primitives.Add(primative);
            }

            // glTF entity can not be empty
            if (mesh.primitives.Count() > 0)
            {
                // Prevent mesh duplication by hash checking
                var meshHash = ManagerUtils.GenerateSHA256Hash(mesh);
                var hs = new ManagerUtils.HashSearch(meshHash);
                var idx = meshContainers.FindIndex(hs.EqualTo);

                if (idx != -1)
                {
                    // set the current nodes mesh index to the already
                    // created mesh location.
                    nodeDict[currentNodeId].mesh = idx;
                }
                else
                {
                    // create new mesh and add it's index to the current node.
                    var mc = new MeshContainer();
                    mc.hashcode = meshHash;
                    mc.contents = mesh;
                    meshContainers.Add(mc);
                    nodeDict[currentNodeId].mesh = meshContainers.Count - 1;
                }
            }

            geometryStack.Pop();
        }

        /// <summary>
        ///     Takes the intermediate geometry data and performs the calculations
        ///     to convert that into glTF buffers, views, and accessors.
        /// </summary>
        /// <param name="geomData"></param>
        /// <param name="name">Unique name for the .bin file that will be produced.</param>
        /// <returns></returns>
        private glTFBinaryData processGeometry(GeometryData geom, string name)
        {
            // TODO: rename this type to glTFBufferMeta ?
            var bufferData = new glTFBinaryData();
            var bufferContents = new glTFBinaryBufferContents();

            foreach (var coord in geom.vertices)
            {
                var vFloat = Convert.ToSingle(coord);
                bufferContents.vertexBuffer.Add(vFloat);
            }

            foreach (var index in geom.faces) bufferContents.indexBuffer.Add(index);

            // Prevent buffer duplication by hash checking
            var calculatedHash = ManagerUtils.GenerateSHA256Hash(bufferContents);
            var hs = new ManagerUtils.HashSearch(calculatedHash);
            var match = binaryFileData.Find(hs.EqualTo);

            if (match != null)
            {
                // return previously created buffer metadata
                bufferData.vertexAccessorIndex = match.vertexAccessorIndex;
                bufferData.indexAccessorIndex = match.indexAccessorIndex;
                return bufferData;
            }

            // add a buffer
            var buffer = new glTFBuffer();
            buffer.uri = name + ".bin";
            buffers.Add(buffer);
            var bufferIdx = buffers.Count - 1;

            /**
                 * Buffer Data
                 **/
            bufferData.name = buffer.uri;
            bufferData.contents = bufferContents;
            // TODO: Uncomment for normals
            //foreach (var normal in geomData.normals)
            //{
            //    bufferData.normalBuffer.Add((float)normal);
            //}

            // Get max and min for vertex data
            var vertexMinMax = Util.GetVec3MinMax(bufferContents.vertexBuffer);
            // Get max and min for index data
            var faceMinMax = Util.GetScalarMinMax(bufferContents.indexBuffer);
            // TODO: Uncomment for normals
            // Get max and min for normal data
            //float[] normalMinMax = getVec3MinMax(bufferData.normalBuffer);

            /**
                 * BufferViews
                 **/
            // Add a vec3 buffer view
            var elementsPerVertex = 3;
            var bytesPerElement = 4;
            var bytesPerVertex = elementsPerVertex * bytesPerElement;
            var numVec3 = geom.vertices.Count / elementsPerVertex;
            var sizeOfVec3View = numVec3 * bytesPerVertex;
            var vec3View = new glTFBufferView();
            vec3View.buffer = bufferIdx;
            vec3View.byteOffset = 0;
            vec3View.byteLength = sizeOfVec3View;
            vec3View.target = Targets.ARRAY_BUFFER;
            bufferViews.Add(vec3View);
            var vec3ViewIdx = bufferViews.Count - 1;

            // TODO: Add a normals (vec3) buffer view

            // Add a faces / indexes buffer view
            var elementsPerIndex = 1;
            var bytesPerIndexElement = 4;
            var bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            var numIndexes = geom.faces.Count;
            var sizeOfIndexView = numIndexes * bytesPerIndex;
            var facesView = new glTFBufferView();
            facesView.buffer = bufferIdx;
            facesView.byteOffset = vec3View.byteLength;
            facesView.byteLength = sizeOfIndexView;
            facesView.target = Targets.ELEMENT_ARRAY_BUFFER;
            bufferViews.Add(facesView);
            var facesViewIdx = bufferViews.Count - 1;

            buffers[bufferIdx].byteLength = vec3View.byteLength + facesView.byteLength;

            /**
                 * Accessors
                 **/
            // add a position accessor
            var positionAccessor = new glTFAccessor();
            positionAccessor.bufferView = vec3ViewIdx;
            positionAccessor.byteOffset = 0;
            positionAccessor.componentType = ComponentType.FLOAT;
            positionAccessor.count = geom.vertices.Count / elementsPerVertex;
            positionAccessor.type = "VEC3";
            positionAccessor.max = new List<float> {vertexMinMax[1], vertexMinMax[3], vertexMinMax[5]};
            positionAccessor.min = new List<float> {vertexMinMax[0], vertexMinMax[2], vertexMinMax[4]};
            accessors.Add(positionAccessor);
            bufferData.vertexAccessorIndex = accessors.Count - 1;

            // TODO: Uncomment for normals
            // add a normals accessor
            //glTFAccessor normalsAccessor = new glTFAccessor();
            //normalsAccessor.bufferView = vec3ViewIdx;
            //normalsAccessor.byteOffset = (positionAccessor.count) * bytesPerVertex;
            //normalsAccessor.componentType = ComponentType.FLOAT;
            //normalsAccessor.count = geom.data.normals.Count / elementsPerVertex;
            //normalsAccessor.type = "VEC3";
            //normalsAccessor.max = new List<float>() { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
            //normalsAccessor.min = new List<float>() { normalMinMax[0], normalMinMax[2], normalMinMax[4] };
            //this.accessors.Add(normalsAccessor);
            //bufferData.normalsAccessorIndex = this.accessors.Count - 1;

            // add a face accessor
            var faceAccessor = new glTFAccessor();
            faceAccessor.bufferView = facesViewIdx;
            faceAccessor.byteOffset = 0;
            faceAccessor.componentType = ComponentType.UNSIGNED_INT;
            faceAccessor.count = numIndexes;
            faceAccessor.type = "SCALAR";
            faceAccessor.max = new List<float> {faceMinMax[1]};
            faceAccessor.min = new List<float> {faceMinMax[0]};
            accessors.Add(faceAccessor);
            bufferData.indexAccessorIndex = accessors.Count - 1;

            bufferData.hashcode = calculatedHash;

            return bufferData;
        }
    }

    internal class Node : glTFNode
    {
        public Element element;
        public string id;
        public int index;
        public bool isFinalized = false;

        public Node(Element elem, int index, bool exportProperties = true, bool isInstance = false,
            string heirarchyFormat = "")
        {
            Debug.WriteLine("{1}  Creating new node: {0}", elem, heirarchyFormat);

            element = elem;
            name = Util.ElementDescription(elem);
            id = isInstance ? elem.UniqueId + "::" + Guid.NewGuid() : elem.UniqueId;
            this.index = index;
            Debug.WriteLine("{1}    Name:{0}", name, heirarchyFormat);

            if (exportProperties)
            {
                // get the extras for this element
                var extras = new glTFExtras();
                extras.UniqueId = elem.UniqueId;

                //var properties = Util.GetElementProperties(elem, true);
                //if (properties != null) extras.Properties = properties;
                extras.Properties = Util.GetElementProperties(elem, true);
                this.extras = extras;
            }

            Debug.WriteLine(string.Format("{0}    Exported Properties", heirarchyFormat));
        }

        public Node(int index)
        {
            name = "::rootNode::";
            id = Guid.NewGuid().ToString();
            this.index = index;
        }

        public glTFNode ToGLTFNode()
        {
            var node = new glTFNode();
            node.name = name;
            node.mesh = mesh;
            node.matrix = matrix;
            node.extras = extras;
            node.children = children;
            return node;
        }
    }
}