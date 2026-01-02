namespace ERH.HeatScans.Reporting.Server.Framework.Models
{
    public class ImageSpot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Temperature { get; set; }
        public ImageSpotPoint Point { get; set; }
    }

    public class ImageSpotPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}