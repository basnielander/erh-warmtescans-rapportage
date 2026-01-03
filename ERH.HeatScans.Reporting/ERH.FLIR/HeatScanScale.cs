using System;

namespace ERH.FLIR;

[Serializable]
public class HeatScanScale
{
    public byte[] Data { get; set; }
    public string MimeType { get; set; }
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
}
