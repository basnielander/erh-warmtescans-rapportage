using Flir.Atlas.Image;
using System.Drawing;
using System.IO;

namespace ERH.FLIR
{
    public class HeatScanImage
    {
        public static bool IsHeatScanImage(Stream imageStream)
        {
            imageStream.Position = 0; // Reset stream position 

            return ThermalImageFile.IsThermalImage(imageStream);
        }

        public static byte[] ImageInBytes(Stream imageStream, bool includeSpotNames = true)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream);
            using var bitmap = thermalImage.Image;

            using var graphics = Graphics.FromImage(bitmap);
            var overlay = new Overlay(thermalImage, includeSpotNames);
            overlay.Draw(graphics);

            // convert to byte array
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public static byte[] AddSpot(Stream imageStream, Spot spot)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream);
            thermalImage.Measurements.Add(new Point(spot.X, spot.Y));

            using var thermalImageStream = new MemoryStream();
            thermalImage.Save(thermalImageStream);

            return ImageInBytes(thermalImageStream);
        }

        public static byte[] CalibrateImage(Stream imageStream, TemperatureScale scale)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream);

            throw new System.NotImplementedException();

        }
    }
}
