using ERH.HeatScans.Reporting.Server.Framework.Services;
using System.IO;
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

                using var rawFileAsStream = await storageService.GetFileBytesAsync(accessToken, imageFileId, cancellationToken);

                var result = heatScanService.GetHeatscanImage(rawFileAsStream);

                return Ok(result);
            });
        }

        /// <summary>
        /// Adds a new spot to the specified heatscan image at the given coordinates.
        /// </summary>
        /// <param name="imageFileId">The unique identifier of the image file to which the spot will be added. Cannot be null or empty.</param>
        /// <param name="relativeX">X, relative to the width of the image</param>
        /// <param name="relativeY">y, relative to the width of the image</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An HTTP response containing the updated heatscan image with the new spot applied.</returns>
        [HttpPost]
        [Route("{imageFileId}/spots")]
        public async Task<IHttpActionResult> AddSpot(string imageFileId, double relativeX, double relativeY, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var rawFileAsStream = await storageService.GetFileBytesAsync(accessToken, imageFileId, cancellationToken);

                var updatedHeatscan = heatScanService.AddSpotToHeatscan(rawFileAsStream, relativeX, relativeY, cancellationToken);

                _ = Task.Run(async () => await storageService.SaveFile(imageFileId, updatedHeatscan.Data, accessToken, cancellationToken));

                using var updatedFileAsStream = new MemoryStream(updatedHeatscan.Data, false);

                var result = heatScanService.GetHeatscanImage(updatedFileAsStream);

                return Ok(result);
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

                foreach (var imageFileId in calibrationRequest.ImageFileIds)
                {
                    var rawFileAsStream = await storageService.GetFileBytesAsync(accessToken, imageFileId, cancellationToken);

                    var updatedHeatscan = heatScanService.CalibrateHeatscan(
                                            rawFileAsStream,
                                            calibrationRequest.MinTemperature,
                                            calibrationRequest.MaxTemperature,
                                            cancellationToken);

                    _ = Task.Run(async () => await storageService.SaveFile(imageFileId, updatedHeatscan.Data, accessToken, cancellationToken));
                }

                return Ok();
            });
        }

        [HttpDelete]
        [Route("{imageFileId}/spots/{name}")]
        public async Task<IHttpActionResult> RemoveSpot(string imageFileId, string name, CancellationToken cancellationToken = default)
        {
            return await ExecuteAuthorizedAsync(async accessToken =>
            {
                var validationResult = ValidateRequired("imageFileId", imageFileId);
                if (validationResult != null)
                {
                    return validationResult;
                }

                validationResult = ValidateRequired("name", name);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var rawFileAsStream = await storageService.GetFileBytesAsync(accessToken, imageFileId, cancellationToken);

                var updatedHeatscan = heatScanService.RemoveSpotFromHeatscan(rawFileAsStream, name, cancellationToken);

                _ = Task.Run(async () => await storageService.SaveFile(imageFileId, updatedHeatscan.Data, accessToken, cancellationToken));

                using var updatedFileAsStream = new MemoryStream(updatedHeatscan.Data, false);

                var result = heatScanService.GetHeatscanImage(updatedFileAsStream);

                return Ok(result);
            });
        }
    }
}