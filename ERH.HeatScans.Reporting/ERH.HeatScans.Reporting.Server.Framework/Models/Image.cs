using ERH.FLIR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERH.HeatScans.Reporting.Server.Framework.Models;

public class Image
{
    public string Id { get; set; }
    public byte[] Data { get; set; }
    public string MimeType { get; set; }

    public byte[] DaylightPhotoData { get; set; }

    public ICollection<ImageSpot> Spots { get; set; }

    public DateTimeOffset DateTaken { get; set; }

    public ImageScale? Scale { get; set; }

    public ImageSize? Size { get; set; }

    public Image()
    {
        Data = [];
        Spots = [];
        MimeType = string.Empty;
        DateTaken = DateTimeOffset.UtcNow;
        Scale = null;
        Size = null;
    }

    public static Image FromHeatscan(HeatScanImage image)
    {
        return new Image
        {
            Data = image.Data,
            MimeType = image.MimeType,
            DaylightPhotoData = image.DaylightPhotoData,
            DateTaken = image.DateTaken,
            Spots = image.Spots.Select(spot => new ImageSpot()
            {
                Id = spot.Id,
                Name = spot.Name,
                Temperature = spot.Temperature,
                Point = new ImageSpotPoint() { X = spot.X, Y = spot.Y }
            }).ToList(),
            Scale = image.ScaleImage != null ? new ImageScale()
            {
                Data = image.ScaleImage.Data,
                MimeType = image.ScaleImage.MimeType,
                MinTemperature = image.ScaleImage.MinTemperature,
                MaxTemperature = image.ScaleImage.MaxTemperature
            } : null,
            Size = ImageSize.FromHeatScanSize(image.Size)
        };
    }
}
