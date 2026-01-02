using Flir.Atlas.Image;
using Flir.Atlas.Image.Palettes;
using System;
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
                Spots = [.. thermalImage.Measurements.MeasurementSpots.Select(ms => new Spot(ms))]
            };
        }

        public static HeatScanImage AddSpot(Stream imageStream, NewSpot spot)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream)
            {
                TemperatureUnit = TemperatureUnit.Celsius,
                DistanceUnit = DistanceUnit.Meter,
                Palette = PaletteManager.Rainbow
            };

            thermalImage.Measurements.Add(new Point(Convert.ToInt32(thermalImage.Width * spot.RelativeX), Convert.ToInt32(thermalImage.Height * spot.RelativeY)));

            foreach (var measurement in thermalImage.Measurements.MeasurementSpots)
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

        public static HeatScanImage RemoveSpot(Stream imageStream, string name)
        {
            imageStream.Position = 0; // Reset stream position 

            using var thermalImage = new ThermalImageFile(imageStream)
            {
                TemperatureUnit = TemperatureUnit.Celsius,
                DistanceUnit = DistanceUnit.Meter,
                Palette = PaletteManager.Rainbow
            };

            // Find the measurement spot with the matching name (which is used as ID)
            var spotToRemove = thermalImage.Measurements.MeasurementSpots
                .FirstOrDefault(ms => ms.Name == name);


            if (spotToRemove != null)
            {
                thermalImage.Measurements.Remove(spotToRemove);
            }

            using var thermalImageStream = new MemoryStream();
            thermalImage.Save(thermalImageStream);

            return ToHeatScanImage(thermalImage, thermalImageStream);
        }
    }
}
