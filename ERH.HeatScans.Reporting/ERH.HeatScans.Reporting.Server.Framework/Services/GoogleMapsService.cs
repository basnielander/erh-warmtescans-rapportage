using Google.Maps.Places.V1;
using Google.Type;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ERH.HeatScans.Reporting.Server.Framework.Services
{
    public class GoogleMapsService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<byte[]> GetStaticMapImage(string address, int zoom, string size, string apiKey, CancellationToken cancellationToken = default)
        {
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
            {
                throw new ArgumentException("Address not found");
            }

            var place = response.Places[0];
            LatLng latLng = place.Location;

            // Validate coordinates
            if (latLng == null || (latLng.Latitude == 0 && latLng.Longitude == 0))
            {
                throw new ArgumentException("Address coordinates not found");
            }

            // Build Google Static Maps URL
            var marker = $"color:red|{latLng.Latitude},{latLng.Longitude}";
            var url =
                $"https://maps.googleapis.com/maps/api/staticmap?center={latLng.Latitude},{latLng.Longitude}&zoom={zoom}&size={size}&markers={Uri.EscapeDataString(marker)}&key={Uri.EscapeDataString(apiKey)}";

            // Download the image and return bytes
            using (var httpResponse = await httpClient.GetAsync(url, cancellationToken))
            {
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to download map image: {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
                }

                return await httpResponse.Content.ReadAsByteArrayAsync();
            }
        }
    }
}