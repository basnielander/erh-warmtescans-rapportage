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

        public FileInfo FrontPage
        {
            get
            {
#if DEBUG
                return new FileInfo(@".\ERH-voorblad.docx");
#else
                return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ERH Warmtescan Rapportage", "ERH-voorblad.docx"));
#endif
            }
        }

        public FileInfo ImagePage
        {
            get
            {
#if DEBUG
                return new FileInfo(@".\ERH-pagina.docx");
#else
                return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ERH Warmtescan Rapportage", "ERH-pagina.docx"));
#endif
            }
        }



        public FileInfo ScaleImageFile
        {
            get
            {
#if DEBUG            
                return new FileInfo(@".\images\temperature-scale.png");
#else
                return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ERH Warmtescan Rapportage", "Images", "temperature-scale.png"));
#endif

            }
        }



    }
}
