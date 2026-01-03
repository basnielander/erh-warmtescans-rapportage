using System;
using System.Collections.Generic;

namespace ERH.HeatScans.Reporting.Server.Framework.Models;

public class Image
{
    public byte[] Data { get; set; }
    public string MimeType { get; set; }

    public ICollection<ImageSpot> Spots { get; set; }

    public DateTimeOffset DateTaken { get; set; }

    public ImageScale? Scale { get; set; }

    public Image()
    {
        Data = [];
        Spots = [];
        MimeType = string.Empty;
        DateTaken = DateTimeOffset.UtcNow;
        Scale = null;
    }
}
