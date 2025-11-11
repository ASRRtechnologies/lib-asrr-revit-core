using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using Autodesk.Revit.DB;
using Util = ASRR.Revit.Core.Exporter.GLTF.Leia.Utils.Util;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.EportUtils
{
    public static class GLTFNodeActions
    {
        public static GLTFNode CreateGLTFNodeFromElement(Element currentElement, Preferences preferences)
        {
            // create a new node for the element
            var newNode = new GLTFNode();
            newNode.name = Util.ElementDescription(currentElement);

            if (preferences.properties)
            {
                // get the extras for this element
                var extras = new GLTFExtras
                {
                    uniqueId = currentElement.UniqueId,
                    parameters = Util.GetElementParameters(currentElement, true)
                };

                if (currentElement.Category != null) extras.elementCategory = currentElement.Category.Name;

#if REVIT2024 || REVIT2025 || REVIT2026
                    extras.elementId = currentElement.Id.Value;
#else
                extras.elementId = currentElement.Id.IntegerValue;
#endif

                newNode.extras = extras;
            }

            return newNode;
        }
    }
}