using System;

namespace NorconsultBridgeStudio.Revit.Core.Exceptions
{
    public class NorconsultBridgeStudioError : Exception
    {
        public NorconsultBridgeStudioError(string message) : base(message)
        {}
    }
}