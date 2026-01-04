using System.Collections.Generic;
using System.IO;

namespace ERH.HeatScans.Reporting.Word;

public class PageProperties
{
    public string ImageName { get; set; }
    public ImageSize ImageSize { get; set; }
    public Stream HeatScanStream { get; set; }
    public Stream DaylightImageStream { get; set; }

    public Stream TemperatureScaleStream { get; set; } = null;
    public Dictionary<string, string> Measurements { get; set; } = [];
    public string Comments { get; set; }
}
