using Flir.Atlas.Image;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace ERH.FLIR
{
    public class Scale
    {
        private const string CELSIUS = "⁰C";
        public Bitmap DrawScale(double minimum, double maximum, FileInfo temperatureScaleImageFile)
        {
            Debug.WriteLine(temperatureScaleImageFile.FullName);
            var bmp = new Bitmap(temperatureScaleImageFile.FullName);

            using var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var g = Graphics.FromImage(bmp))
            {
                using var font = new Font("Verdana", 10);

                g.DrawString($"{maximum}{CELSIUS}", font, Brushes.Black, new Rectangle(0, 0, bmp.Width, 24), stringFormat);
                g.DrawString($"{minimum}{CELSIUS}", font, Brushes.Black, new Rectangle(0, 280 - 24, bmp.Width, 24), stringFormat);
            }

            return bmp;
        }

        public Bitmap DrawScale(ThermalImageFile thermalImage)
        {
            using var bmp = new Bitmap(thermalImage.Size.Width, thermalImage.Size.Height);
            using var scaleImage = new Bitmap(thermalImage.Scale.Image, new Size(thermalImage.Scale.Image.Width * 25, thermalImage.Scale.Image.Height));

            using var g = Graphics.FromImage(bmp);

            //Use gdi+ to interpolate with nearest neighbor
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(scaleImage, 0, 0, bmp.Width * 2, bmp.Height);

            var scaleWithTempRangeBitmap = new Bitmap((thermalImage.Scale.Image.Width * 25) + 200, thermalImage.Scale.Image.Height + 100);

            using var scaleWithTempRangeImage = Graphics.FromImage(scaleWithTempRangeBitmap);

            scaleWithTempRangeImage.Clear(Color.Black);
            scaleWithTempRangeBitmap.MakeTransparent(Color.Black);
            scaleWithTempRangeImage.SmoothingMode = SmoothingMode.AntiAlias;
            scaleWithTempRangeImage.InterpolationMode = InterpolationMode.HighQualityBicubic;

            scaleWithTempRangeImage.DrawImage(scaleImage, new Point(100, 25));

            return scaleWithTempRangeBitmap;
        }

        public Bitmap DrawScale2(ThermalImageFile thermalImage)
        {
            using var bmp = new Bitmap(thermalImage.Size.Width, thermalImage.Size.Height);
            var scaleImage = thermalImage.Scale.Image;
            using (var g = Graphics.FromImage(bmp))
            {
                //Use gdi+ to interpolate with nearest neighbor
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(scaleImage, 0, 0, bmp.Width * 2, bmp.Height);
            }

            return scaleImage;
        }
    }
}