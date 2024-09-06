using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace NorconsultBridgeStudio.Revit.Core.Exceptions
{
    public class WarningSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
        {
            IList<FailureMessageAccessor> failures = a.GetFailureMessages();

            foreach (FailureMessageAccessor f in failures)
            {
                FailureSeverity failureSeverity = a.GetSeverity();

                if ((int)failureSeverity > (int)FailureSeverity.Warning)
                    return FailureProcessingResult.ProceedWithRollBack;

                a.DeleteWarning(f); //simply catch all warnings, so you don't have to find out what warning is causing the message to pop up
            }
            return FailureProcessingResult.Continue;
        }
    }
}