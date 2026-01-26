using ERH.FLIR;

namespace ERH.HeatScans.Reporting.Server.Framework.Models;

public class ImageSize(int width, int height)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;

    public static ImageSize FromHeatScanSize(HeatScanSize size) => new(size.Width, size.Height);
}