using System.Collections.Generic;

namespace ERH.HeatScans.Reporting.Server.Framework.Models
{
    public class Report
    {
        public string FolderId { get; set; }
        public List<ImageInfo> Images { get; set; }

        public Report()
        {
            FolderId = string.Empty;
            Images = new List<ImageInfo>();
        }
    }

    public class ImageInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public long? Size { get; set; }
        public string ModifiedTime { get; set; }

        public ImageInfo()
        {
            Id = string.Empty;
            Name = string.Empty;
            MimeType = string.Empty;
            ModifiedTime = string.Empty;
        }
    }
}
