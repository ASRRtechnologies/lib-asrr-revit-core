using ASRR.Revit.Core.RevitModel;
using ASRR.Revit.Core.Warnings;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASRR.Revit.Core.Utilities { 

    public static class GroupUtilities
    {
        public static void CreateGroup(Document doc, string groupName)
        {
            var elements = ModelElementCollector.GetParentModelElements(doc).ToList();
            var ids = elements.Select(e => e.Id).ToList();

            using (var transaction = WarningDiscardFailuresPreprocessor.GetTransaction(doc))
            {
                transaction.Start("Create Group");
                try
                {
                    if (ids.Count > 0)
                    {
                        var group = doc.Create.NewGroup(ids);
                        group.GroupType.Name = groupName;

                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.RollBack();
                    throw;
                }
            }
        }

        public static void CreateGroup(Document doc, List<ElementId> elementIds, string groupName)
        {
            var elements = ModelElementCollector.GetParentModelElements(doc).ToList();
            var ids = elements.Where(e => elementIds.Contains(e.Id)).Select(e => e.Id).ToList();

            using (var transaction = WarningDiscardFailuresPreprocessor.GetTransaction(doc))
            {
                transaction.Start("Create Group");
                try
                {
                    if (ids.Count > 0)
                    {
                        var group = doc.Create.NewGroup(ids);
                        group.GroupType.Name = groupName;
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.RollBack();
                    throw;
                }
            }
        }
    }
}
