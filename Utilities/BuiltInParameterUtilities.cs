using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using NLog;
using System;
using System.Linq;
using ASRR.Revit.Core.Elements.Families;

namespace ASRR.Revit.Core.Elements.Parameters
{
    public static class BuiltInParameterUtilities
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

       

        public static string GetAssemblyCode(Autodesk.Revit.DB.Element element)
        {
            try
            {
                string assemblyCode = element.get_Parameter(BuiltInParameter.UNIFORMAT_CODE).AsString();
                Log.Debug($"assemblyCode is '{assemblyCode}'");
                return assemblyCode;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetAssemblyName(Autodesk.Revit.DB.Element element)
        {
            try
            {
                string assemblyName = element.get_Parameter(BuiltInParameter.UNIFORMAT_DESCRIPTION).AsString();
                Log.Debug($"assemblyName is '{assemblyName}'");
                return assemblyName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetMaterial(Document doc, Element element)
        {
            var FPR = element as HostObject;
            
            if (FPR?.GetType() == null)
                return element.GetParameters("Structural Material")[0].AsValueString();

            switch (FPR.GetType().Name)
            {
                case "Wall":
                    var wall = element as Wall;
                    return doc.GetElement(wall.WallType.GetCompoundStructure().GetMaterialId(0)).Name;// as Material;
                case "Floor":
                    var floor = element as Floor;
                    return doc.GetElement(floor.FloorType.GetCompoundStructure().GetMaterialId(0)).Name;// as Material;
                case "FootPrintRoof":
                    var roof = element as FootPrintRoof;
                    return doc.GetElement(roof.RoofType.GetCompoundStructure().GetMaterialId(0)).Name;// as Material;
                case "Fascia":
                    var fascia = element as Fascia;
                    return doc.GetElement(fascia.FasciaType.GetCompoundStructure().GetMaterialId(0)).Name;// as Material;
                default:
                    var mat = doc.GetElement(element.GetMaterialIds(true).First()).Name; // as Material;
                    Log.Debug("returnd default value");
                    return mat;
            }
        }
    }
}
