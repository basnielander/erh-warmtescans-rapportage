using Flir.Atlas.Image.Measurements;

namespace ERH.FLIR
{
    public class Spot
    {
        public MeasurementShape Shape;

        public Spot(MeasurementShape shape)
        {
            Shape = shape;
        }

        public string Name => Shape.Name;

        public string Temperature => $"{Shape.GetValues()[0].ToString("F1")}⁰C";
    }
}
