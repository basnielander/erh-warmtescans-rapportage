namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    public class ReportDetailsUpdate
    {
        public string Address { get; set; }
        public double? Temperature { get; set; }
        public double? WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public double? HoursOfSunshine { get; set; }
        public string FrontDoorDirection { get; set; }
    }
}