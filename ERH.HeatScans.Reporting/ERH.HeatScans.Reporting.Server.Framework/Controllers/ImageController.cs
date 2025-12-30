using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/image")]
    public class ImageController : ApiController
    {
        [HttpPost]
        [Route("spot")]
        public async Task<IHttpActionResult> AddSpot(string imageFileId, int x, int y, CancellationToken cancellationToken = default)
        {
            try
            {
                //var accessToken = GetAccessTokenFromHeader();
                //if (string.IsNullOrEmpty(accessToken))
                //{
                //    return Unauthorized();
                //}
                //await storageService.SetupAddressFolderAsync(accessToken, addressFolderId, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}