namespace ERH.HeatScans.Reporting.Server.Framework.Models
{
    public class FileDownloadResult
    {
        public byte[] Data { get; set; }
        public string MimeType { get; set; }

        public FileDownloadResult()
        {
            Data = new byte[0];
            MimeType = string.Empty;
        }
    }
}
