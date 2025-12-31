using ERH.HeatScans.Reporting.Server.Framework.Services;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/folders-and-files")]
    public class FoldersAndFilesController : ApiController
    {
        private readonly GoogleDriveService storageService;

        public FoldersAndFilesController() : base()
        {
            storageService = new GoogleDriveService();
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
                var accessToken = AccessToken.Get(Request);
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
                var accessToken = AccessToken.Get(Request);
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
                var accessToken = AccessToken.Get(Request);
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


    }
}
