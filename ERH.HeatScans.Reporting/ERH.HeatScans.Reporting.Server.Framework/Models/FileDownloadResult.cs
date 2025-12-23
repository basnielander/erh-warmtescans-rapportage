namespace ERH.HeatScans.Reporting.Server.Framework.Models
{
    public class FileDownloadResult
    {
        public byte[] Data { get; set; }
        public string MimeType { get; set; }

        public FileDownloadResult()
        {
            MimeType = string.Empty;
        }
    }
}
