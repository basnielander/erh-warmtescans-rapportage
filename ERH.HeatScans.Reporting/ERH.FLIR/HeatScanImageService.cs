using Flir.Atlas.Image;
using Flir.Atlas.Image.Measurements;
using Flir.Atlas.Image.Palettes;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ERH.FLIR
{
    public class HeatScanImageService
    {
        public static bool IsHeatScanImage(Stream imageStream)
        {
            imageStream.Position = 0; // Reset stream position 

            return ThermalImageFile.IsThermalImage(imageStream);
        }

        public static HeatScanImage ImageInBytes(Stream imageStream, bool includeSpotNames = true)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream)
            {
                TemperatureUnit = TemperatureUnit.Celsius,
                DistanceUnit = DistanceUnit.Meter,
                Palette = PaletteManager.Rainbow
            };

            using var bitmap = thermalImage.Image;

            using var graphics = Graphics.FromImage(bitmap);
            var overlay = new Overlay(thermalImage, includeSpotNames);
            overlay.Draw(graphics);

            // convert to byte array
            using var thermalImageStream = new MemoryStream();
            bitmap.Save(thermalImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            return ToHeatScanImage(thermalImage, thermalImageStream);
        }

        private static HeatScanImage ToHeatScanImage(ThermalImageFile thermalImage, MemoryStream thermalImageStream)
        {
            return new HeatScanImage()
            {
                Data = thermalImageStream.ToArray(),
                MimeType = "image/jpeg",
                DateTaken = thermalImage.DateTaken,
                Spots = [.. thermalImage.Measurements.OfType<MeasurementSpot>().Select(ms => new Spot(ms))]
            };
        }

        public static HeatScanImage AddSpot(Stream imageStream, Spot spot)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream)
            {
                TemperatureUnit = TemperatureUnit.Celsius,
                DistanceUnit = DistanceUnit.Meter,
                Palette = PaletteManager.Rainbow
            };
            thermalImage.Measurements.Add(new Point(spot.X, spot.Y));

            foreach (var measurement in thermalImage.Measurements.OfType<MeasurementSpot>())
            {
                Debug.WriteLine($"Spot {measurement.Name}, temp: {measurement.ThermalParameters.ReflectedTemperature}, x: {measurement.X}, y: {measurement.Y}");
            }

            using var thermalImageStream = new MemoryStream();
            thermalImage.Save(thermalImageStream);

            return ToHeatScanImage(thermalImage, thermalImageStream);
        }

        public static HeatScanImage CalibrateImage(Stream imageStream, TemperatureScale scale)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream)
            {
                TemperatureUnit = TemperatureUnit.Celsius,
                DistanceUnit = DistanceUnit.Meter,
                Palette = PaletteManager.Rainbow
            };

            thermalImage.Scale.Range = new Range<double>(scale.Min, scale.Max);

            using var thermalImageStream = new MemoryStream();
            thermalImage.Save(thermalImageStream);

            return ToHeatScanImage(thermalImage, thermalImageStream);
        }
    }
}
