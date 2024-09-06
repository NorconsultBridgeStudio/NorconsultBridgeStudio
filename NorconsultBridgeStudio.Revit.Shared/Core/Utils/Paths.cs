using System.IO;

namespace NorconsultBridgeStudio.Revit.Core.Utils
{
    public static class Paths
    {
        private static string lastDirectory = "";
        private static string lastCrossSection = "";

        public static string LastDirectory { get => lastDirectory; set => lastDirectory = value; }

        public static string LastCrossSection { get => lastCrossSection; set => lastCrossSection = value; }

        public static string GetAdaptivePath()
        {
            string templatepath = App.UIApplication.Application.FamilyTemplatePath;
            string adaptivePath = Path.Combine(templatepath, "Metric Generic Model Adaptive.rft");
            return adaptivePath;
        }
        
    }
}