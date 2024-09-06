using NorconsultBridgeStudio.Revit.Core.Utils;
using System.Windows.Media.Imaging;
using System;

namespace NorconsultBridgeStudio.Revit.Core.Utils
{
    public static class ResourceUtils
    {
        public static BitmapImage GetImageOrDefault(string imageName)
        {
            imageName = string.IsNullOrEmpty(imageName) ? "greyButton" : imageName;

            var iconsPath = $"pack://application:,,,/{Constants.ThisAssemblyName};component/Icons";
            var imageUri = new Uri($"{iconsPath}/{imageName}.png");

            BitmapImage image;

            try
            {
                image = new BitmapImage(imageUri);
            }
            catch
            {
                imageUri = new Uri($"{iconsPath}/greyButton.png");
                image = new BitmapImage(imageUri);
            }

            return image;
        }
    }
}