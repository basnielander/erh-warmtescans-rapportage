using ERH.HeatScans.Reporting.Server.Framework.Services;
using System;
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
    }
}