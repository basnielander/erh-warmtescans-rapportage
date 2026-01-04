using System;
using System.Collections.Generic;

namespace ERH.HeatScans.Reporting
{
    public class Report(string folderId, string address)
    {
        private Report() : this("", "")
        { }

        public string FolderId { get; set; } = folderId;
        public string Address { get; set; } = address;

        public DateTimeOffset? PhotosTakenAt { get; set; }

        public IList<ReportImage> Images { get; set; } = [];

        public double? Temperature { get; set; }

        public double? WindSpeed { get; set; }

        public string WindDirection { get; set; }

        public double? HoursOfSunshine { get; set; }

        public string FrontDoorDirection { get; set; }
    }
}
