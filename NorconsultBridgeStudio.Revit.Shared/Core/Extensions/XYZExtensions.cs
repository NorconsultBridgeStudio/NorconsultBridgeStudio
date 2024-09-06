using Autodesk.Revit.DB;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Extensions
{
    public static class XYZExtensions
    {
        /// <summary>
        /// Projects a point to plan, i.e. Z-component is set to 0.
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ ToPlan(this XYZ xyz)
        {
            return new XYZ(xyz.X, xyz.Y, 0);
        }
    }
}