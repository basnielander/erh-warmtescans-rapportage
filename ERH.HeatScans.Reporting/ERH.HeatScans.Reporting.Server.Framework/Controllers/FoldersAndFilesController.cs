using ERH.HeatScans.Reporting.Server.Framework.Services;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/folders-and-files")]
    public class FoldersAndFilesController : ApiController
    {
        private readonly GoogleDriveService storageService;
        private readonly FLIRService heatScanService;

        public FoldersAndFilesController() : base()
        {
            storageService = new GoogleDriveService();
            heatScanService = new FLIRService();
        }

        /// <summary>
        /// Get the hierarchical folder and file structure from user's Google Drive
        /// Requires user authentication via Google OAuth
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Folder structure with children</returns>
        [HttpGet]
        [Route("users")]
        public async Task<IHttpActionResult> GetFolderStructure(CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = GetAccessTokenFromHeader();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }

                var usersFolderId = ConfigurationManager.AppSettings["UsersFolderId"];

                if (string.IsNullOrWhiteSpace(usersFolderId))
                {
                    return BadRequest("UsersFolderId is not configured.");
                }

                var structure = await storageService.GetFolderStructureAsync(accessToken, usersFolderId, CancellationToken.None);
                return Ok(structure);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get a flat list of all files from user's Google Drive folder
        /// Requires user authentication via Google OAuth
        /// </summary>
        /// <param name="folderId">Optional folder ID to start from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Flat list of files</returns>
        [HttpGet]
        [Route("files")]
        public async Task<IHttpActionResult> GetFiles(string folderId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = GetAccessTokenFromHeader();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }

                var files = await storageService.GetFlatFileListAsync(accessToken, folderId, cancellationToken);
                return Ok(files);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> SetupAddressFolder(string addressFolderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = GetAccessTokenFromHeader();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }
                await storageService.SetupAddressFolderAsync(accessToken, addressFolderId, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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
                var accessToken = GetAccessTokenFromHeader();
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

        [HttpGet]
        [Route("image")]
        public async Task<IHttpActionResult> GetImage(string fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = GetAccessTokenFromHeader();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(fileId))
                {
                    return BadRequest("fileId is required");
                }

                var rawFileInBytes = await storageService.GetFileBytesAsync(accessToken, fileId, cancellationToken);

                var result = heatScanService.GetHeatscanImage(rawFileInBytes);

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(result.Data)
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(result.MimeType);

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private string GetAccessTokenFromHeader()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null || authHeader.Scheme != "Bearer")
            {
                return null;
            }

            return authHeader.Parameter;
        }
    }
}
