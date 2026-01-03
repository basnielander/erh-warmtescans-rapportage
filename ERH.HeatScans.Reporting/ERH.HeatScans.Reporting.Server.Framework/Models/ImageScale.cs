namespace ERH.HeatScans.Reporting.Server.Framework.Models;

public class ImageScale
{
    public byte[] Data { get; set; }
    public string MimeType { get; set; }
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
}