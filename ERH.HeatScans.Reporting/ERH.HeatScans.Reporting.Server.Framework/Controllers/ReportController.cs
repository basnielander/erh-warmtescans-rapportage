using ERH.HeatScans.Reporting.Server.Framework.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/report")]
    public class ReportController : AuthorizedApiController
    {
        private readonly GoogleDriveService storageService;
        private readonly ReportService reportService;
        private readonly FLIRService heatScanService;

        public ReportController() : base()
        {
            storageService = new GoogleDriveService();
            reportService = new ReportService();
            heatScanService = new FLIRService();
        }

        /// <summary>
        /// Get report for a specific address folder
        /// Retrieves or creates report.json in the "2. Bewerkt" subfolder
        /// </summary>
        /// <param name="folderId">Address folder ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Report with folder ID and list of images</returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetReport(string folderId = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("folderId", folderId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var report = await storageService.GetReportAsync(accessToken, folderId, cancellationToken);
                return Ok(report);
            });
        }

        /// <summary>
        /// Update image indices in the report
        /// </summary>
        /// <param name="folderId">Address folder ID</param>
        /// <param name="indexUpdates">List of image index updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPost]
        [Route("update-indices")]
        public async Task<IHttpActionResult> UpdateImageIndices(string folderId, [FromBody] List<ImageIndexUpdateRequest> indexUpdates, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("folderId", folderId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                if (indexUpdates == null || !indexUpdates.Any())
                {
                    return BadRequest("indexUpdates are required");
                }

                await storageService.UpdateImageIndicesAsync(accessToken, folderId, indexUpdates, cancellationToken);
                return Ok();
            });
        }

        /// <summary>
        /// Toggle image exclusion from the report
        /// </summary>
        /// <param name="imageId">HeatScanImage file ID to toggle</param>
        /// <param name="exclude">True to exclude, false to include</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPost]
        [Route("toggle-image-exclusion")]
        public async Task<IHttpActionResult> ToggleImageExclusion(string imageId, bool exclude, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("imageId", imageId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                await storageService.ToggleImageExclusionAsync(accessToken, imageId, exclude, cancellationToken);
                return Ok();
            });
        }

        /// <summary>
        /// Update image properties (comment and outdoor)
        /// </summary>
        /// <param name="imageId">HeatScanImage file ID to update</param>
        /// <param name="comment">Comment text</param>
        /// <param name="outdoor">True if outdoor, false if indoor</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPatch]
        [Route("update-image-properties")]
        public async Task<IHttpActionResult> UpdateImageProperties(string imageId, string comment, bool outdoor, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("imageId", imageId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                await storageService.UpdateImagePropertiesAsync(accessToken, imageId, comment, outdoor, cancellationToken);
                return Ok();
            });
        }

        /// <summary>
        /// Update image temperature calibration
        /// </summary>
        /// <param name="imageId">HeatScanImage file ID to update</param>
        /// <param name="temperatureMin">Minimum temperature in Celsius</param>
        /// <param name="temperatureMax">Maximum temperature in Celsius</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPost]
        [Route("update-image-calibration")]
        public async Task<IHttpActionResult> UpdateImageCalibration(string imageId, double temperatureMin, double temperatureMax, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("imageId", imageId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                if (temperatureMin >= temperatureMax)
                {
                    return BadRequest("temperatureMin must be less than temperatureMax");
                }

                await storageService.UpdateImageCalibrationAsync(accessToken, imageId, temperatureMin, temperatureMax, cancellationToken);
                return Ok();
            });
        }

        /// <summary>
        /// Update report details (address, weather conditions, etc.)
        /// </summary>
        /// <param name="folderId">Folder ID</param>
        /// <param name="reportDetails">Report details to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPatch]
        [Route("update-report-details")]
        public async Task<IHttpActionResult> UpdateReportDetails(string folderId, [FromBody] ReportDetailsUpdateRequest reportDetails, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("folderId", folderId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                validationResult = ValidateRequired("reportDetails", reportDetails);
                if (validationResult != null)
                {
                    return validationResult;
                }

                await storageService.UpdateReportDetailsAsync(accessToken, folderId, reportDetails, cancellationToken);
                return Ok();
            });
        }

        [HttpPost]
        [Route("document")]
        public async Task<IHttpActionResult> CreateReport(string folderId = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("folderId", folderId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var report = await storageService.GetReportAsync(accessToken, folderId, cancellationToken);

                if (report == null)
                {
                    return BadRequest("Report not found");
                }

                var heatScans = new List<Models.Image>();

                var orderedIncludedImages = report.Images.Where(img => !img.ExcludeFromReport).OrderBy(img => img.Index).ToList();

                foreach (var image in orderedIncludedImages)
                {
                    using var rawFileAsStream = await storageService.GetFileBytesAsync(accessToken, image.Id, cancellationToken);

                    var heatScan = heatScanService.GetHeatscanImage(rawFileAsStream);
                    heatScan.Id = image.Id;

                    heatScans.Add(heatScan);
                }

                var reportDocument = reportService.CreateReportDocumentAsync(folderId, report, heatScans);

                _ = Task.Run(async () => await storageService.CreateOrUpdateFile($"{report.Address} - Warmtescanrapport {report.PhotosTakenAt.Value.ToString("dd-MM-yyyy")}.docx", folderId, reportDocument, accessToken, cancellationToken));

                return Ok(reportDocument);
            });
        }
    }
}