using ERH.HeatScans.Reporting.Server.Framework.Services;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/maps")]
    public class MapsController : ApiController
    {
        private readonly GoogleMapsService mapsService;

        public MapsController() : base()
        {
            mapsService = new GoogleMapsService();
        }

        // GET api/maps/image?address=...&zoom=16&size=600x400
        [HttpGet]
        [Route("image")]
        public async Task<IHttpActionResult> GetStaticMapImage(string address, int zoom = 19, string size = "600x400", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(address))
                return BadRequest("Address is required.");

            if (!address.Contains(","))
            {
                address += ", Houten";
            }

            address += ", Netherlands";

            // Try to get API key from AppSettings first, then fall back to environment variable
            var apiKey = ConfigurationManager.AppSettings["GoogleMapsApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("GoogleMapsApiKey");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
                return InternalServerError(new InvalidOperationException("Google Maps API key is missing in configuration (appSettings: GoogleMapsApiKey) and environment variables."));
            try
            {
                var bytes = await mapsService.GetStaticMapImage(address, zoom, size, apiKey, cancellationToken);

                var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return ResponseMessage(result);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception exc)
            {
                return InternalServerError(exc);
            }
        }
    }
}