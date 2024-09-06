using Autodesk.Revit.UI;

namespace NorconsultBridgeStudio.Revit.Core.Interfaces
{
    public interface INorconsultBridgeStudioCommand : IExternalCommand
    {
        string ToolTip { get; }
        string IconName { get; }
        string PublicName { get; }
        string CommandAvailability { get; }
        void RunCommand();
    }
}