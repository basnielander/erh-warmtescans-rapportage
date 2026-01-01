using ERH.HeatScans.Reporting.Server.Framework.Models;
using ERH.HeatScans.Reporting.Server.Framework.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/images")]
    public class ImageController : AuthorizedApiController
    {
        private readonly GoogleDriveService storageService;
        private readonly FLIRService heatScanService;

        public ImageController() : base()
        {
            storageService = new GoogleDriveService();
            heatScanService = new FLIRService();
        }

        [HttpGet]
        [Route("{imageFileId}")]
        public async Task<IHttpActionResult> GetImage(string imageFileId, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("imageFileId", imageFileId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var rawFileAsStream = await storageService.GetFileBytesAsync(accessToken, imageFileId, cancellationToken);

                var result = heatScanService.GetHeatscanImage(rawFileAsStream);

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(result.Data)
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(result.MimeType);

                return ResponseMessage(response);
            });
        }

        [HttpPost]
        [Route("{imageFileId}/spots")]
        public async Task<IHttpActionResult> AddSpot(string imageFileId, int x, int y, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var rawFileAsStream = await storageService.GetFileBytesAsync(accessToken, imageFileId, cancellationToken);

                FileDownloadResult updatedHeatscan = heatScanService.AddSpotToHeatscan(rawFileAsStream, x, y, cancellationToken);

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(updatedHeatscan.Data)
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(updatedHeatscan.MimeType);

                return ResponseMessage(response);
            });
        }

        [HttpPost]
        [Route("calibrate")]
        public async Task<IHttpActionResult> CalibrateImages([FromBody] CalibrationRequest calibrationRequest, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("calibrationRequest", calibrationRequest);
                if (validationResult != null)
                {
                    return validationResult;
                }

                if (calibrationRequest.ImageFileIds == null || calibrationRequest.ImageFileIds.Count == 0)
                {
                    return BadRequest("ImageFileIds are required");
                }

                if (calibrationRequest.MinTemperature >= calibrationRequest.MaxTemperature)
                {
                    return BadRequest("MinTemperature must be less than MaxTemperature");
                }

                await storageService.BatchUpdateImageCalibrationAsync(
                    accessToken, 
                    calibrationRequest.ImageFileIds.ToList(), 
                    calibrationRequest.MinTemperature, 
                    calibrationRequest.MaxTemperature, 
                    cancellationToken);

                return Ok();
            });
        }
    }
}