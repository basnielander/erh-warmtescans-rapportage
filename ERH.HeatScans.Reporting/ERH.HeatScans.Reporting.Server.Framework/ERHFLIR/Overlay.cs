using Flir.Atlas.Image;
using Flir.Atlas.Image.Measurements;
using System.Drawing;

namespace ERH.FLIR
{
    public class Overlay
    {
        private readonly static int fontSize = 8;

        public Overlay(ThermalImageFile image, bool includeSpotName)
        {
            _image = image;
            this.includeSpotName = includeSpotName;
        }

        public void Draw(Graphics graphics)
        {
            foreach (MeasurementSpot spot in _image.Measurements.MeasurementSpots)
            {
                DrawSpot(spot, graphics);
            }
            foreach (MeasurementRectangle rectangle in _image.Measurements.MeasurementRectangles)
            {
                DrawArea(rectangle, graphics);
            }
            foreach (MeasurementLine line in _image.Measurements.MeasurementLines)
            {
                DrawLine(line, graphics);
            }
        }

        private void DrawSpot(MeasurementSpot spot, Graphics graphics)
        {
            int dist = 10;
            int r = 4;
            graphics.DrawEllipse(_pen, spot.X - r / 2, spot.Y - r / 2, r, r);

            var p1 = new Point(spot.X, spot.Y - dist);
            var p2 = new Point(spot.X, spot.Y - r);
            graphics.DrawLine(_pen, p1, p2);

            var p3 = new Point(spot.X, spot.Y + r);
            var p4 = new Point(spot.X, spot.Y + dist);
            graphics.DrawLine(_pen, p3, p4);

            p1 = new Point(spot.X - dist, spot.Y);
            p2 = new Point(spot.X - r, spot.Y);
            graphics.DrawLine(_pen, p1, p2);

            p3 = new Point(spot.X + r, spot.Y);
            p4 = new Point(spot.X + dist, spot.Y);
            graphics.DrawLine(_pen, p3, p4);

            if (includeSpotName)
            {
                using var _textFont = new Font(FontFamily.GenericSansSerif, fontSize);
                graphics.DrawString(spot.Name, _textFont, _textBrush, spot.X + r, spot.Y - 2 * r - fontSize);
            }
        }

        private static Rectangle PadHitArea(Point pt)
        {
            return new Rectangle(pt.X - 3, pt.Y - 3, 6, 6);
        }

        private void DrawAreaSelection(MeasurementRectangle rectangle, Graphics graphics)
        {
            Rectangle selection = PadHitArea(new Point(rectangle.Location.X, rectangle.Location.Y));

            // Upper left
            graphics.DrawLine(_pen, selection.Location, new Point(selection.X + selection.Width, selection.Y));
            graphics.DrawLine(_pen, selection.Location, new Point(selection.X, selection.Y + selection.Height));

            // Upper mid
            selection = PadHitArea(new Point(rectangle.Location.X + rectangle.Width / 2, rectangle.Location.Y));

            graphics.DrawLine(_pen, selection.Location, new Point(selection.X + selection.Width, selection.Y));
            graphics.DrawLine(_pen, new Point(selection.Location.X, selection.Y + selection.Height), new Point(selection.X + selection.Width, selection.Y + selection.Height));

            // Upper right
            selection = PadHitArea(new Point(rectangle.Location.X + rectangle.Width, rectangle.Location.Y));

            graphics.DrawLine(_pen, selection.Location, new Point(selection.X + selection.Width, selection.Y));
            graphics.DrawLine(_pen, new Point(selection.X + selection.Width, selection.Y), new Point(selection.X + selection.Width, selection.Y + selection.Height));


            // Right mid
            selection = PadHitArea(new Point(rectangle.Location.X + rectangle.Width, rectangle.Location.Y + rectangle.Height / 2));

            graphics.DrawLine(_pen, selection.Location, new Point(selection.X, selection.Y + selection.Height));
            graphics.DrawLine(_pen, new Point(selection.X + selection.Width, selection.Y), new Point(selection.X + selection.Width, selection.Y + selection.Height));

            // Bottom right
            selection = PadHitArea(new Point(rectangle.Location.X + rectangle.Width, rectangle.Location.Y + rectangle.Height));

            graphics.DrawLine(_pen, new Point(selection.Location.X + selection.Width, selection.Location.Y), new Point(selection.Location.X + selection.Width, selection.Y + selection.Height));
            graphics.DrawLine(_pen, new Point(selection.Location.X + selection.Width, selection.Y + selection.Height), new Point(selection.X, selection.Y + selection.Height));

            // Bottom mid
            selection = PadHitArea(new Point(rectangle.Location.X + rectangle.Width / 2, rectangle.Location.Y + rectangle.Height));

            graphics.DrawLine(_pen, selection.Location, new Point(selection.Location.X + selection.Width, selection.Y));
            graphics.DrawLine(_pen, new Point(selection.Location.X, selection.Y + selection.Height), new Point(selection.Location.X + selection.Width, selection.Y + selection.Height));

            // Lower left
            selection = PadHitArea(new Point(rectangle.Location.X, rectangle.Location.Y + rectangle.Height));

            graphics.DrawLine(_pen, selection.Location, new Point(selection.Location.X, selection.Y + selection.Height));
            graphics.DrawLine(_pen, new Point(selection.Location.X, selection.Y + selection.Height), new Point(selection.Location.X + selection.Width, selection.Y + selection.Height));

            // Left mid
            selection = PadHitArea(new Point(rectangle.Location.X, rectangle.Location.Y + rectangle.Height / 2));

            graphics.DrawLine(_pen, selection.Location, new Point(selection.Location.X, selection.Y + selection.Height));
            graphics.DrawLine(_pen, new Point(selection.Location.X + selection.Width, selection.Y), new Point(selection.Location.X + selection.Width, selection.Y + selection.Height));
        }

        private void DrawArea(MeasurementRectangle rectangle, Graphics graphics)
        {
            Rectangle rect = new Rectangle(rectangle.Location.X, rectangle.Location.Y, rectangle.Width, rectangle.Height);
            graphics.DrawRectangle(_pen, rect);

            DrawAreaSelection(rectangle, graphics);

            string str = rectangle.Min.Value.ToString("F01");
            str += " - ";
            str += rectangle.Max.Value.ToString("F01");

            using var _textFont = new Font(FontFamily.GenericSansSerif, fontSize);
            graphics.DrawString(str, _textFont, _textBrush, rectangle.Location.X + 5, rectangle.Location.Y + 5);
        }

        private void DrawLineSelection(MeasurementLine line, Graphics graphics)
        {
            // Left mid
            Rectangle selection = PadHitArea(line.Start);
            graphics.DrawRectangle(_pen, selection);

            selection = PadHitArea(line.End);
            graphics.DrawRectangle(_pen, selection);

            string str = line.Min.Value.ToString("F01");
            str += " - ";
            str += line.Max.Value.ToString("F01");

            using var _textFont = new Font(FontFamily.GenericSansSerif, fontSize);
            graphics.DrawString(str, _textFont, _textBrush, line.Start.X + 5, line.Start.Y + 5);
        }

        private void DrawLine(MeasurementLine line, Graphics graphics)
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.DrawLine(_pen, line.Start, line.End);
            DrawLineSelection(line, graphics);
        }

        private ThermalImageFile _image;
        private readonly bool includeSpotName;
        protected Pen _pen = Pens.White;
        protected Brush _textBrush = Brushes.White;
    }
}
