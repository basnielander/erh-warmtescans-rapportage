using ERH.HeatScans.Reporting.Server.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/report")]
    public class ReportController : ApiController
    {
        private readonly GoogleDriveService storageService;

        public ReportController() : base()
        {
            storageService = new GoogleDriveService();
        }

        /// <summary>
        /// Get report for a specific address folder
        /// Retrieves or creates report.json in the "2. Bewerkt" subfolder
        /// </summary>
        /// <param name="folderId">Address folder ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Report with folder ID and list of images</returns>
        [HttpGet]
        [Route("report")]
        public async Task<IHttpActionResult> GetReport(string folderId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = AccessToken.Get(Request);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(folderId))
                {
                    return BadRequest("folderId is required");
                }

                var report = await storageService.GetReportAsync(accessToken, folderId, cancellationToken);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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
        public async Task<IHttpActionResult> UpdateImageIndices(string folderId, [FromBody] List<ImageIndexUpdate> indexUpdates, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = AccessToken.Get(Request);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(folderId))
                {
                    return BadRequest("folderId is required");
                }

                if (indexUpdates == null || !indexUpdates.Any())
                {
                    return BadRequest("indexUpdates are required");
                }

                await storageService.UpdateImageIndicesAsync(accessToken, folderId, indexUpdates, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Toggle image exclusion from the report
        /// </summary>
        /// <param name="imageId">Image file ID to toggle</param>
        /// <param name="exclude">True to exclude, false to include</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPost]
        [Route("toggle-image-exclusion")]
        public async Task<IHttpActionResult> ToggleImageExclusion(string imageId, bool exclude, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = AccessToken.Get(Request);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(imageId))
                {
                    return BadRequest("imageId is required");
                }

                await storageService.ToggleImageExclusionAsync(accessToken, imageId, exclude, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    public class ImageIndexUpdate
    {
        public string Id { get; set; }
        public int Index { get; set; }
    }
}