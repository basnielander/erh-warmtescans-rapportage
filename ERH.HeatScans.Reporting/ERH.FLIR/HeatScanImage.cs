using Flir.Atlas.Image;
using System.Drawing;

namespace ERH.FLIR
{
    public class HeatScanImage
    {
        private readonly Bitmap bitmap;

        public HeatScanImage(ThermalImageFile thermalImageFile)
        {
            bitmap = thermalImageFile.Image.Clone() as Bitmap;
        }

        public Bitmap Image(ThermalImageFile _image)
        {
            using var graphics = Graphics.FromImage(bitmap);
            var overlay = new Overlay(_image, true);
            overlay.Draw(graphics);

            return bitmap;
        }
    }
}
