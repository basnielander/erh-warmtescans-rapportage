using Flir.Atlas.Image;
using Flir.Atlas.Image.Palettes;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ERH.FLIR;

public class HeatScanImageService
{
    public static bool IsHeatScanImage(Stream imageStream)
    {
        imageStream.Position = 0;
        return ThermalImageFile.IsThermalImage(imageStream);
    }

    public static HeatScanImage ImageInBytes(Stream imageStream, bool includeSpotNames = true)
    {
        using var thermalImage = CreateThermalImage(imageStream);
        using var bitmap = thermalImage.Image;
        using var graphics = Graphics.FromImage(bitmap);

        var overlay = new Overlay(thermalImage, includeSpotNames);
        overlay.Draw(graphics);

        using var thermalImageStream = new MemoryStream();
        bitmap.Save(thermalImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);

        return ToHeatScanImage(thermalImage, thermalImageStream);
    }

    public static HeatScanImage AddSpot(Stream imageStream, NewSpot spot)
    {
        using var thermalImage = CreateThermalImage(imageStream);

        thermalImage.Measurements.Add(new Point(Convert.ToInt32(thermalImage.Width * spot.RelativeX), Convert.ToInt32(thermalImage.Height * spot.RelativeY)));

        foreach (var measurement in thermalImage.Measurements.MeasurementSpots)
        {
            Debug.WriteLine($"Spot {measurement.Name}, temp: {measurement.Value.Value}, x: {measurement.X}, y: {measurement.Y}");
        }

        return SaveAndConvert(thermalImage);
    }

    public static HeatScanImage CalibrateImage(Stream imageStream, TemperatureScale scale)
    {
        using var thermalImage = CreateThermalImage(imageStream);

        thermalImage.Scale.Range = new Range<double>(scale.Min, scale.Max);

        return SaveAndConvert(thermalImage);
    }

    public static HeatScanImage RemoveSpot(Stream imageStream, string name)
    {
        using var thermalImage = CreateThermalImage(imageStream);

        foreach (var measurement in thermalImage.Measurements.MeasurementSpots)
        {
            Debug.WriteLine($"Before Spot {measurement.Name}, temp: {measurement.Value.Value}, x: {measurement.X}, y: {measurement.Y}, point: ({measurement.Point.X}, {measurement.Point.Y})");
        }

        var spotToRemove = thermalImage.Measurements.MeasurementSpots
            .FirstOrDefault(ms => ms.Name == name);

        if (spotToRemove != null)
        {
            thermalImage.Measurements.Remove(spotToRemove);
        }

        foreach (var measurement in thermalImage.Measurements.MeasurementSpots)
        {
            Debug.WriteLine($"After Spot {measurement.Name}, temp: {measurement.Value.Value}, x: {measurement.X}, y: {measurement.Y}, point: ({measurement.Point.X}, {measurement.Point.Y})");
        }

        return SaveAndConvert(thermalImage);
    }

    private static ThermalImageFile CreateThermalImage(Stream imageStream)
    {
        imageStream.Position = 0;

        return new ThermalImageFile(imageStream)
        {
            TemperatureUnit = TemperatureUnit.Celsius,
            DistanceUnit = DistanceUnit.Meter,
            Palette = PaletteManager.Rainbow
        };
    }

    private static HeatScanImage SaveAndConvert(ThermalImageFile thermalImage)
    {
        using var thermalImageStream = new MemoryStream();
        thermalImage.Save(thermalImageStream);
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
}
