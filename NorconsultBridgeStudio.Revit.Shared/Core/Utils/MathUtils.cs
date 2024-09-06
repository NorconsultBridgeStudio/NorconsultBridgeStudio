using System;

namespace NorconsultBridgeStudio.Revit.Core.Utils
{
    public static class MathUtils
    {
        public static bool AreFloatsEqual(double a, double b, double tolerance = 1e-9)
        {
            return Math.Abs(a - b) <= tolerance;
        }
    }
}