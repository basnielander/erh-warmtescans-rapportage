using System;
using System.Collections.Generic;

namespace ERH.HeatScans.Reporting.Server.Framework.Models
{
    public class GoogleDriveItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public bool IsFolder { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public long? Size { get; set; }
        public List<GoogleDriveItem> Children { get; set; }

        public GoogleDriveItem()
        {
            Id = string.Empty;
            Name = string.Empty;
            MimeType = string.Empty;
            Children = new List<GoogleDriveItem>();
        }
    }
}
