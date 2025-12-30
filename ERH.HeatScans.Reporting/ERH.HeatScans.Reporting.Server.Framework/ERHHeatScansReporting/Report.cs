using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ERH.HeatScans.Reporting
{
    public class Report
    {
        public Report()
        {

        }

        public string FolderId { get; set; }

        private void SetToolVersion()
        {
            ToolVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString() ?? "";
        }

        public string ToolVersion { get; set; }

        public DirectoryInfo ImagesFolder { get; internal set; }

        public IList<ReportImage> Images { get; set; } = [];

        public IReadOnlyList<ReportImage> SortedImages
        {
            get
            {
                return Images.OrderBy(i => i.Index).ToList();
            }
        }



        public string Address { get; set; }

        public double Temperature { get; set; }

        public double WindSpeed { get; set; }

        public string WindDirection { get; set; }

        public double HoursOfSunshine { get; set; }

        public string FrontDoorDirection { get; set; }

        public TemperatureScale TemperatureScale { get; set; }

        public string TemperatureScaleImage { get; set; }
    }
}
