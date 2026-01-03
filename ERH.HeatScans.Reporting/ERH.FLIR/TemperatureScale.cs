using System;

namespace ERH.FLIR;

[Serializable]
public class TemperatureScale(double min, double max)
{
    public double Max { get; set; } = max;
    public double Min { get; set; } = min;
}
