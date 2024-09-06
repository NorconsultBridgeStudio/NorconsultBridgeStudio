using Autodesk.Revit.DB;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
namespace NorconsultBridgeStudio.Revit.Core.Extensions
{
    public static class TransactionExtensions
    {
        /// <summary>
        /// Suprress warnings from a transaction. If this is called, the user will not be notified of any warnings that arises from the transaction.
        /// </summary>
        /// <param name="transaction"></param>
        public static void SuppressWarnings(this Transaction transaction)
        {
            FailureHandlingOptions failOpt = transaction.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            transaction.SetFailureHandlingOptions(failOpt);
        }
    }
}
