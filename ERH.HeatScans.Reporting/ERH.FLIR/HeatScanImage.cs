using System;
using System.Collections.Generic;

namespace ERH.FLIR
{
    [Serializable]
    public class HeatScanImage
    {
        public byte[] Data { get; set; }
        public string MimeType { get; set; }

        public ICollection<Spot> Spots { get; set; }

        public DateTimeOffset DateTaken { get; set; }

        public HeatScanImage()
        {
            Data = [];
            Spots = [];
            MimeType = string.Empty;
            DateTaken = DateTimeOffset.UtcNow;
        }
    }
}
