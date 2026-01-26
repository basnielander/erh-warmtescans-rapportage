using ERH.HeatScans.Reporting.Server.Framework.Models;
using ERH.HeatScans.Reporting.Word;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ERH.HeatScans.Reporting.Server.Framework.Services
{
    public class ReportService
    {
        public byte[] CreateReportDocumentAsync(string folderId, Report report, List<Image> heatScans)
        {
            using var reportDocumentGenerator = new ReportDocumentCreator();

            using var frontPagePhotoStream = new MemoryStream(heatScans.First().DaylightPhotoData);

            report.PhotosTakenAt = heatScans.First().DateTaken.ToString("dd-MM-yyyy");

            reportDocumentGenerator.CreateDocumentWithoutTheImages(report, frontPagePhotoStream);

            foreach (var heatScan in heatScans)
            {
                var reportImage = report.Images.First(img => img.Id == heatScan.Id);

                try
                {
                    // cancellationToken.ThrowIfCancellationRequested();

                    var pageProperties = new PageProperties
                    {
                        Comments = reportImage.Comments,
                        DaylightImageStream = new MemoryStream(heatScan.DaylightPhotoData),
                        HeatScanStream = new MemoryStream(heatScan.Data),
                        ImageName = reportImage.Name,
                        ImageSize = GetImageSize(heatScan.Data),
                        Measurements = heatScan.Spots.ToDictionary(
                            m => m.Name,
                            m => m.Temperature),
                        TemperatureScaleStream = new MemoryStream(heatScan.Scale.Data)
                    };
                    reportDocumentGenerator.AddPage(pageProperties);

                }
                catch (System.Exception exc)
                {
                    Debug.WriteLine($"Error adding image {reportImage.Name} to report: {exc.Message}");
                    // Log error but continue with next image
                }

            }

            using var documentStream = reportDocumentGenerator.Save();
            return documentStream.ToArray();
        }

        private Word.ImageSize GetImageSize(byte[] data)
        {
            using var imageStream = new MemoryStream(data);
            var image = System.Drawing.Image.FromStream(imageStream);

            return new(image.Width, image.Height);
        }
    }
}