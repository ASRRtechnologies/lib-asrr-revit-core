using ASRR.Revit.Core.Elements.Rotation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using ParameterUtils = ASRR.Revit.Core.Elements.Parameters.ParameterUtils;

namespace ASRR.Revit.Core.Elements.Placement
{
    /// <summary>
    ///     Class to interact with families
    /// </summary>
    public class FamilyPlacer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     This method places a family based on reference plane, location and referenceDirection
        /// </summary>
        public void Place(Document doc,
            XYZ location,
            ElementId levelId,
            FamilySymbol symbol,
            double rotation,
            bool mirrored,
            Dictionary<string, object> parameters)
        {
            var level = doc.GetElement(levelId) as Level;

            using (var transaction = new Transaction(doc))
            {
                transaction.Start("Place family instance");
                var newFamilyInstance =
                    doc.Create.NewFamilyInstance(location, symbol, level, StructuralType.NonStructural);
                Log.Debug(
                    $"Placed new family instance at {location} on level {level?.Elevation}, id is '{newFamilyInstance.Id}'");
                var instanceLocation = ((LocationPoint)newFamilyInstance.Location).Point;

                if (rotation != 0.0)
                {
                    Log.Debug($"Rotating element {rotation} degrees");
                    ElementRotator.RotateElement(newFamilyInstance, rotation, location);
                }

                if (mirrored)
                {
                    Log.Debug("Mirroring element");
                    using (var plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, location)) // ZX
                    {
                        ElementTransformUtils.MirrorElements(doc, new[] { newFamilyInstance.Id }, plane, false);
                    }
                }

                ParameterUtils.Apply(newFamilyInstance, parameters);

                transaction.Commit();
            }
        }


        /// <summary>
        ///     This method places a family based on curve (line based elements)
        /// </summary>
        public void Place(Document doc,
            Curve curve,
            ElementId levelId,
            FamilySymbol symbol,
            double rotation,
            bool mirrored,
            Dictionary<string, object> parameters)
        {
            var level = doc.GetElement(levelId) as Level;

            using (var transaction = new Transaction(doc))
            {
                transaction.Start("Place family instance");
                var newFamilyInstance =
                    doc.Create.NewFamilyInstance(curve, symbol, level, StructuralType.NonStructural);
                Log.Debug(
                    $"Placed new family instance based on curve {curve.GetEndPoint(0)} {curve.Length} on level {level?.Elevation}, id is '{newFamilyInstance.Id}'");

                if (mirrored)
                {
                    Log.Debug("Mirroring element");
                    using (var plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, curve.GetEndPoint(0))) // ZX
                    {
                        ElementTransformUtils.MirrorElements(doc, new[] { newFamilyInstance.Id }, plane, false);
                    }
                }

                ParameterUtils.Apply(newFamilyInstance, parameters);

                transaction.Commit();
            }
        }

        public List<FamilySymbol> GetFamilySymbol(Document doc, string typeName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(x => x.Name.Equals(typeName))
                    .ToList(); // family type         
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<FamilyInstance> GetFamilyInstancesByFamilyName(Document doc, string familyName)
        {
            try
            {
                var familyInstances = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(x => x.Symbol.Family.Name.Contains(familyName));
                return familyInstances;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<FamilyInstance> GetFamilyInstancesByFamilyAndType(Document doc, string familyName,
            string typeName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(x => x.Symbol.Family.Name.Equals(familyName)) // family
                    .Where(x => x.Name.Equals(typeName)); // family type         
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}