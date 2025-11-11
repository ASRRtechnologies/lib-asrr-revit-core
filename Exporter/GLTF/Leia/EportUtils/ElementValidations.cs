using ASRR.Revit.Core.Exporter.GLTF.Leia.Core;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Model;
using ASRR.Revit.Core.Exporter.GLTF.Leia.Utils;
using Autodesk.Revit.DB;
using System.Linq;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.EportUtils
{
    public static class ElementValidations
    {
        public static bool ShouldSkipElement(Element currentElement, View currentView,
            Document currentDocument, Preferences preferences, IndexedDictionary<GLTFNode> nodes)
        {
            if (currentElement == null) return true;

            var isHiddenOrLocked = !Util.CanBeLockOrHidden(currentElement, currentView, currentDocument.IsFamilyDocument);
            var isLevelToSkip = currentElement is Level && !preferences.levels;
            var isAlreadyProcessed = nodes.Contains(currentElement.UniqueId);

            if (isHiddenOrLocked || isLevelToSkip || isAlreadyProcessed) return true;

            return false;
        }

        public static bool ShouldOmitElement(Element currentElement,
            IndexedDictionary<VertexLookupIntObject> currentVertices,
            View currentView, Document currentDocument, ElementId elemId)
        {
            if (currentElement == null)
                return true;

#if REVIT2026
                if (currentElement.Id.Value != elemId.Value)
                    return true;
#else
            if (currentElement.Id.IntegerValue != elemId.IntegerValue)
                return true;
#endif


            if (currentVertices == null || !currentVertices.List.Any())
                return true;

            if (!Util.CanBeLockOrHidden(currentElement, currentView, currentDocument.IsFamilyDocument) ||
                currentElement is RevitLinkInstance)
                return true;

            return false;
        }
    }
}