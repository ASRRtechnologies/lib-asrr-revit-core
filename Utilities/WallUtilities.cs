using ASRR.Revit.Core.Elements.Families;
using Autodesk.Revit.DB;
using System;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using Autodesk.Revit.ApplicationServices;
using ASRR.Revit.Core.Elements;

namespace ASRR.Revit.Core.Utilities
{
    public class WallUtilities
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static bool IsConstructive(Wall wall)
        {
            if (wall == null) throw new ArgumentNullException(nameof(wall));

            var utils = new RevitFamilyUtils();

            // var family = utils.GetFamilyFromInstance(element, true);
            // if (family == null) return false;

            var wallType = wall.WallType;
            // Log.Info($"Walltype for element {element.Name} is {wallType.Name}");
            var parameter = wall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM);
            if (parameter == null) return false;

            Log.Debug($"Found parameter for element {wall.Name}: {parameter.AsValueString()}");
            return parameter.AsValueString() == "Bearing";
        }

        public static int WallLength(Wall wall)
        {
            var parameter = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
            // Log.Info($"element length is: {parameter}");
            return Convert.ToInt32(parameter);
        }

        public static int WallHeight(Wall wall)
        {
            var parameter = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsValueString();
            // Log.Info($"element height is: {parameter}");
            return Convert.ToInt32(parameter);
        }

        public static string WallBaseConstraint(Wall wall)
        {
            var parameter = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsValueString();
            Log.Debug($"element base constraint is: {parameter}");
            return parameter;
        }

        public static Curve GetCurve(Wall wall)
        {
            if (!(wall.Location is LocationCurve location)) throw new Exception("Could not cast element to LocationCurve");
            return location.Curve;
        }

        public static Tuple<double, double> GetWallDimensions(Wall wall)
        {
            var curve = GetCurve(wall);
            var length = curve.Length;
            var height = WallHeight(wall);
            return new Tuple<double, double>(length, height);
        }

        public static Tuple<XYZ, XYZ> GetEndPoints(Wall wall)
        {
            var curve = GetCurve(wall);
            var start = curve.GetEndPoint(0);
            var end = curve.GetEndPoint(1);
            return new Tuple<XYZ, XYZ>(start, end);
        }

        public static double GetNormalAxisProjection(Wall wall, double theta)
        {
            if (!(wall.Location is LocationCurve c))
                throw new Exception("Could not cast element to LocationCurve");

            var xyzFeet = c.Curve.GetEndPoint(0); // 0 and 1 will give same output due to projection on normal axis
            var xyz = CoordinateUtilities.ConvertFeetToMm(xyzFeet);

            return Math.Cos(theta) * xyz.X + Math.Sin(theta) * xyz.Y;
        }

        public static bool IsExterior(WallType wallType)
        {
            var p = wallType.get_Parameter(
                BuiltInParameter.FUNCTION_PARAM);

            var f = (WallFunction)p.AsInteger();

            return WallFunction.Exterior == f;
        }

        public List<Face> GetAllWallFaces(Element element)
        {
            List<Face> facesLst = new List<Face>();
            ReferenceArray refArray = new ReferenceArray();
            // string faceInfo = "";
            Options opt = new Options();
            opt.ComputeReferences = true;
            GeometryElement geomElem = element.get_Geometry(opt);
            foreach (GeometryObject geomObj in geomElem)
            {
                Solid geomSolid = geomObj as Solid;
                if (null != geomSolid)
                {
                    int faces = 0;
                    double totalArea = 0;
                    foreach (Face geomFace in geomSolid.Faces)
                    {
                        Face face = geomFace as Face;
                        refArray.Append(face.Reference);
                        //Log.Info($"--------------- {face.Reference.ElementReferenceType} ---------------");
                        facesLst.Add(face);
                        faces++;
                        // faceInfo += "Face " + faces + " area: " + geomFace.Area.ToString() + "\n";
                        totalArea += geomFace.Area;
                    }
                }
            }
            return facesLst;
        }


        public static bool Intersects(Wall wall, bool mirrored, ReferenceIntersector referenceIntersector, Document doc)
        {
            var orientation = wall.Orientation;
            var inverseNormal = mirrored ? orientation : GeometryUtils.GetInverse(orientation);
            var quarters = GeometryUtils.GetWallClashPoints(wall);

            var addition = 0.1f;

            foreach (var context in quarters.Select(q => q + (inverseNormal * addition)).Select(pointInFrontOfFace =>
                         referenceIntersector.FindNearest(pointInFrontOfFace, inverseNormal)))
            {
                if (context != null && doc.GetElement(context.GetReference()) is Wall w &&
                    !IsExterior(w.WallType))
                    return false;

                if (context != null) Log.Trace($"Prox: {CoordinateUtilities.ConvertFeetToMm(context.Proximity)}");

                //TODO: deze 500 variabele vangen in settings
                var result = context != null && CoordinateUtilities.ConvertFeetToMm(context.Proximity) < 500;
                if (result)
                    return true;
            }

            return false;
        }

        public static XYZ GetNormalizedDirection(Wall wall)
        {
            var lc = wall.Location as LocationCurve;
            var direction = (lc.Curve.GetEndPoint(0) - lc.Curve.GetEndPoint(1)).Normalize();
            return direction;
        }
    }
}