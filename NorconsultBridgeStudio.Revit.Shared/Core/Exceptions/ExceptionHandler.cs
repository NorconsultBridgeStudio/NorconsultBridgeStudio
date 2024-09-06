using Autodesk.Revit.UI;
using NorconsultBridgeStudio.Revit.Core.Utils;
using System;

namespace NorconsultBridgeStudio.Revit.Core.Exceptions
{
    public static class ExceptionHandler
    {
        /// <summary>
        /// The method handles exceptions in the command class of Bruvit functions. In the case of an unknown exception the user will be prompted with an option to notify/email the developers. 
        /// In cases of known errors/self thrown warnings, only guidance will be given to the user.
        /// </summary>
        /// <param name="ex">The exception to handle</param>
        /// <returns></returns>
        public static Result HandleException(Exception ex, string functionName)
        {
            if (ex is Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            else if (ex is NorconsultBridgeStudioError)
            {
                TaskDialog.Show("Bruvit", ex.Message);
                return Result.Failed;
            }
            else
            {
                var bruvitDialog = new NorconsultBridgeStudioErrorDialog(ex, functionName);
                var dialogResult = bruvitDialog.Show();

                return Result.Failed;
            }
        }
    }
}