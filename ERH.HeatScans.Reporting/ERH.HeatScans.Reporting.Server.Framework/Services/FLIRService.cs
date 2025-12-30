using ERH.FLIR;
using ERH.HeatScans.Reporting.Server.Framework.Models;

namespace ERH.HeatScans.Reporting.Server.Framework.Services
{
    public class FLIRService
    {
        internal FileDownloadResult GetHeatscanImage(byte[] imageInBytes)
        {
            //if (!HeatScanImage.IsHeatScanImage(imageInBytes))
            //{
            //    throw new InvalidDataException("The provided file is not a valid heat scan image.");
            //}

            return new FileDownloadResult
            {
                Data = HeatScanImage.ImageInBytes(imageInBytes, true),
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