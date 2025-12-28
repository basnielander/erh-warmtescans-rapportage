using Flir.Atlas.Image;
using System.Drawing;

namespace ERH.FLIR
{
    public class DaylightImage
    {
        public DaylightImage(ThermalImageFile thermalImageFile)
        {
            thermalImageFile.Fusion.Mode = thermalImageFile.Fusion.VisualOnly;

            GetDaylightImage = thermalImageFile.Fusion.VisualImage;
        }

        public Bitmap GetDaylightImage { get; private set; }
    }
}
