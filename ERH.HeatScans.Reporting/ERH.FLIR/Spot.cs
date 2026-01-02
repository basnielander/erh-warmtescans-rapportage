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

        public int X { get; }
        public int Y { get; }
        //public MeasurementShape Shape;

        //public Spot(MeasurementShape shape)
        //{
        //    Shape = shape;
        //}

        //public string Name => Shape.Name;

        //public string Temperature => $"{Shape.GetValues()[0].ToString("F1")}⁰C";
    }
}
