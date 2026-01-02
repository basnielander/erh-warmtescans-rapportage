using System;

namespace ERH.FLIR
{
    [Serializable]
    public class NewSpot
    {
        public NewSpot(double relativeX, double relativeY)
        {
            RelativeX = relativeX;
            RelativeY = relativeY;
        }

        public double RelativeX { get; }
        public double RelativeY { get; }


    }
}
