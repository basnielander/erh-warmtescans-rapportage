using System.Collections.Generic;

namespace ERH.HeatScans.Reporting
{
    public class Report
    {

        public string FolderId { get; set; }

        public IList<ReportImage> Images { get; set; } = [];

        public string Address { get; set; }

        public double Temperature { get; set; }

        public double WindSpeed { get; set; }

        public string WindDirection { get; set; }

        public double HoursOfSunshine { get; set; }

        public string FrontDoorDirection { get; set; }

        //public TemperatureScale TemperatureScale { get; set; }

        //public string TemperatureScaleImage { get; set; }
    }
}
