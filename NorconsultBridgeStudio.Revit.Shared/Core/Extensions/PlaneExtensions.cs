using Autodesk.Revit.DB;
using System;

namespace NorconsultBridgeStudio.Revit.Core.Extensions
{
    public static class PlaneExtensions
    {
        /// <summary>
        /// Check if two planes are conincident within a given distance tolerance.
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="otherNormal">The normal of the other plane</param>
        /// <param name="otherOrigin">The origin of the other plane</param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsCoincidentWith(this Plane plane, XYZ otherOrigin, XYZ otherNormal, double tolerance = 1e-9)
        {
            return (plane.Normal.IsAlmostEqualTo(otherNormal) || plane.Normal.IsAlmostEqualTo(otherNormal.Negate())) && Math.Abs(plane.SignedDistanceTo(otherOrigin)) < tolerance;

        }
        /// <summary>
        /// The signed distance from the plane to this point.
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double SignedDistanceTo(this Plane plane, XYZ p)
        {
            // https://thebuildingcoder.typepad.com/blog/2014/09/planes-projections-and-picking-points.html#12

            XYZ v = p - plane.Origin;

            return plane.Normal.Normalize().DotProduct(v);
        }
    }
}