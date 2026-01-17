using System;

namespace ERH.FLIR;

[Serializable]
public class HeatScanSize(int width, int height)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
}
