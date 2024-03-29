﻿using Autodesk.Revit.DB;

namespace ASRR.Revit.Core
{
    public class UseDestinationHandler : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            return DuplicateTypeAction.UseDestinationTypes;
        }

        public static CopyPasteOptions UseDestinationOnDuplicateNameCopyPasteOptions()
        {
            var copyOptions = new CopyPasteOptions();
            copyOptions.SetDuplicateTypeNamesHandler(new UseDestinationHandler());
            return copyOptions;
        }
    }
}