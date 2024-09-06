using System.Reflection;

namespace NorconsultBridgeStudio.Revit.Core.Utils
{
    class Constants
    {
        public const string TabName = "Bridge Studio";
        public const string BridgeModellingPanelName = "Bridge Modelling";
        public const string WatermarkParameter = "NorconsultBridgeStudio Watermark";
        public const string WatermarkFormula = "\"NorconsultBridgeStudio Create Superstructure 1.0\"";
        public const string WatermarkValue = "NorconsultBridgeStudio Create Superstructure 1.0";
        public const string FamilyBasePointX = "FamilyBasePointX";
        public const string FamilyBasePointY = "FamilyBasePointY";
        public const string FamilyBasePointZ = "FamilyBasePointZ";
        public const string Material = "Material";
        public static string ThisAssemblyPath => Assembly.GetExecutingAssembly().Location;
        public static string ThisAssemblyName => Assembly.GetExecutingAssembly().GetName().Name;
        public static double FeetToMillimeter = 304.8;
        public static double MillimeterToFeet = 1/FeetToMillimeter;
        public static double MeterToFeet = MillimeterToFeet*1000;
    }
}