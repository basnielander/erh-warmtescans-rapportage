using System.Collections.Generic;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    public class CalibrationRequest
    {
        public ICollection<string> ImageFileIds { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
    }
}