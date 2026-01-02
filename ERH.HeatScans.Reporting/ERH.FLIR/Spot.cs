using Flir.Atlas.Image.Measurements;
using System;

namespace ERH.FLIR
{
    [Serializable]
    public class Spot
    {
        public Spot(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Spot(MeasurementSpot spot)
        {
            Id = spot.Identity.ToString();
            Name = spot.Name;
            Temperature = $"{spot.Value.Value.ToString("F1")}⁰C";
            X = spot.X;
            Y = spot.Y;
        }

        public int X { get; }
        public int Y { get; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Temperature { get; set; }

    }
}
