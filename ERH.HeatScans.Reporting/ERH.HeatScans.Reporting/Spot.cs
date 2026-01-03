namespace ERH.HeatScans.Reporting;

public class Spot
{
    public Spot(string name, double temperature, int x, int y)
    {
        Name = name;
        Temperature = temperature;
        X = x;
        Y = y;
    }

    public string TemperatureDisplay => $"{Temperature:F1}⁰C";

    public string Name { get; }
    public double Temperature { get; }
    public int X { get; }
    public int Y { get; }
}
