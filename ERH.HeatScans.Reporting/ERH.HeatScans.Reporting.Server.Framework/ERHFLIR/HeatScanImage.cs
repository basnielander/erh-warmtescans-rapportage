using Flir.Atlas.Image;
using System.Drawing;
using System.IO;

namespace ERH.FLIR
{
    public class HeatScanImage
    {
        public static bool IsHeatScanImage(Stream image)
        {
            image.Position = 0; // Reset stream position 

            return ThermalImageFile.IsThermalImage(image);
        }

        public static byte[] ImageInBytes(Stream image, bool includeSpotNames = true)
        {
            image.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(image);
            using var bitmap = thermalImage.Image;

            using var graphics = Graphics.FromImage(bitmap);
            var overlay = new Overlay(thermalImage, includeSpotNames);
            overlay.Draw(graphics);

            // convert to byte array
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
    }
}
