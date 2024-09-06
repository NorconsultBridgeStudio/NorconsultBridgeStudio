using Autodesk.Revit.DB;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Utils
{
    class AlignmentLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            source = FamilySource.Family;
            return true;
        }
    }
}