﻿using System.Collections.Generic;
using ASRR.Revit.Core.Exporter.GLTF.Core;
using ASRR.Revit.Core.Exporter.GLTF.Model;
using ASRR.Revit.Core.Exporter.GLTF.Utils;
using Autodesk.Revit.DB;

namespace ASRR.Revit.Core.Exporter.GLTF.Export
{
    /// <summary>
    /// Revit grids.
    /// </summary>
    public static class RevitGrids
    {
        /// <summary>
        /// Export Revit grids.
        /// </summary>
        /// <param name="doc">Revit document.</param>
        /// <param name="nodes">Nodes.</param>
        /// <param name="rootNode">root node.</param>
        /// <param name="preferences">preferences. </param>
        public static void Export(Document doc, ref IndexedDictionary<GLTFNode> nodes, ref GLTFNode rootNode, Preferences preferences)
        {
            using (FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Grid)))
            {
                var grids = col.ToElements();
                for (int i = 0; i < grids.Count; i++)
                {
                    Grid g = grids[i] as Grid;
                    Line l = g.Curve as Line;

                    // TODO: handle Arc, not only Line
                    if (l == null)
                    {
                        continue;
                    }

                    var origin = l.Origin;
                    var direction = l.Direction;
                    var length = l.Length;

                    var xtras = new GLTFExtras();
                    var grid = new RevitGridParametersObject();

                    grid.origin = new List<double>() { origin.X, origin.Y, origin.Z };

                    grid.direction = new List<double>() { direction.X, direction.Y, direction.Z, };
                    grid.length = length;

                    xtras.gridParameters = grid;
                    xtras.uniqueId = g.UniqueId;

                    if (preferences.properties)
                    {
                        xtras.parameters = Util.GetElementParameters(g, true);
                    }

                    var gridNode = new GLTFNode();
                    gridNode.name = g.Name;
                    gridNode.extras = xtras;

                    nodes.AddOrUpdateCurrent(g.UniqueId, gridNode);
                    rootNode.children.Add(nodes.CurrentIndex);
                }
            }
        }
    }
}
