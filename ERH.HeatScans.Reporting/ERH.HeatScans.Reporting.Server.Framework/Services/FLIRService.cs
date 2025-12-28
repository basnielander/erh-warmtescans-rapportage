using ERH.FLIR;
using ERH.HeatScans.Reporting.Server.Framework.Models;
using System.IO;

namespace ERH.HeatScans.Reporting.Server.Framework.Services
{
    public class FLIRService
    {
        internal FileDownloadResult GetHeatscanImage(Stream fileStream)
        {
            if (!HeatScanImage.IsHeatScanImage(fileStream))
            {
                throw new InvalidDataException("The provided file is not a valid heat scan image.");
            }

            return new FileDownloadResult
            {
                Data = HeatScanImage.ImageInBytes(fileStream, true),
                MimeType = "image/jpeg"
            };
            //return new FileDownloadResult
            //{
            //    Data = [],
            //    MimeType = "image/jpeg"
            //};
        }
    }
}