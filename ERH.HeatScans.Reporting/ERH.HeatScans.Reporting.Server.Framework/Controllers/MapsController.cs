using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Google.Maps.Places.V1;
using Google.Type;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    [RoutePrefix("api/maps")]
    public class MapsController : ApiController
    {
        // GET api/maps/image?address=...&zoom=16&size=600x400
        [HttpGet]
        [Route("image")]
        public IHttpActionResult GetStaticMapImage(string address, int zoom = 19, string size = "600x400", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(address))
                return BadRequest("Address is required.");

            if (!address.Contains(",", StringComparison.OrdinalIgnoreCase))
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

            // Create Places client using API key
            var client = new PlacesClientBuilder { ApiKey = apiKey }.Build();            

            var request = new SearchTextRequest
            {
                TextQuery = address,
                LanguageCode = "nl",
                RegionCode = "NL",
            };

            var response = client.SearchText(request, new Google.Api.Gax.Grpc.CallSettings(cancellationToken, null, null, header => header.Add(new("X-Goog-FieldMask", "*")), null, null, null, null));
            if (response?.Places == null || response.Places.Count == 0)
                return NotFound();

            var place = response.Places[0];
            LatLng latLng = place.Location;

            // Validate coordinates
            if (latLng == null || (latLng.Latitude == 0 && latLng.Longitude == 0))
                return NotFound();

            // Build Google Static Maps URL
            var marker = $"color:red|{latLng.Latitude},{latLng.Longitude}";
            var url =
                $"https://maps.googleapis.com/maps/api/staticmap?center={latLng.Latitude},{latLng.Longitude}&zoom={zoom}&size={size}&markers={Uri.EscapeDataString(marker)}&key={Uri.EscapeDataString(apiKey)}";

            // Download the image and return bytes
            using var httpClient = new HttpClient();
            
                var httpResponse = httpClient.GetAsync(url, cancellationToken).Result;
                if (!httpResponse.IsSuccessStatusCode)
                    return InternalServerError(new HttpRequestException($"Failed to download map image: {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}"));

                var bytes = httpResponse.Content.ReadAsByteArrayAsync().Result;
                var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return ResponseMessage(result);
            
        }
    }
}