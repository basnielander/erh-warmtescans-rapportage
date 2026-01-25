using ERH.Weather.Client.Models;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ERH.Weather.Client
{
    /// <summary>
    /// Client for retrieving historical weather data from the Open-Meteo Archive API.
    /// </summary>
    public class HistoricalWeatherClient : IDisposable
    {
        private const string BaseUrl = "https://archive-api.open-meteo.com/v1/archive";
        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient;

        /// <summary>
        /// Initializes a new instance of the HistoricalWeatherClient class with a default HttpClient.
        /// </summary>
        public HistoricalWeatherClient()
        {
            _httpClient = new HttpClient();
            _disposeHttpClient = true;
        }

        /// <summary>
        /// Initializes a new instance of the HistoricalWeatherClient class with a provided HttpClient.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use for API requests.</param>
        public HistoricalWeatherClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _disposeHttpClient = false;
        }

        /// <summary>
        /// Retrieves historical weather data for a specific location and date/time.
        /// </summary>
        /// <param name="location">The geographic location for which to retrieve weather data.</param>
        /// <param name="dateTime">The date and time for which to retrieve weather data.</param>
        /// <returns>A WeatherData object containing the historical weather information.</returns>
        public async Task<WeatherData> GetHistoricalWeatherAsync(WeatherLocation location, DateTime dateTime)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            var date = dateTime.Date;
            var url = BuildUrl(location, date, date);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = DeserializeResponse(json);

            return MapToWeatherData(apiResponse, location, dateTime);
        }

        /// <summary>
        /// Builds the API URL with the required parameters.
        /// </summary>
        private string BuildUrl(WeatherLocation location, DateTime startDate, DateTime endDate)
        {
            var latitude = location.Latitude.ToString("F4", CultureInfo.InvariantCulture);
            var longitude = location.Longitude.ToString("F4", CultureInfo.InvariantCulture);
            var start = startDate.ToString("yyyy-MM-dd");
            var end = endDate.ToString("yyyy-MM-dd");

            var hourlyParams = "temperature_2m,relative_humidity_2m,precipitation,wind_speed_10m,wind_direction_10m,pressure_msl,cloud_cover";

            return $"{BaseUrl}?latitude={latitude}&longitude={longitude}&start_date={start}&end_date={end}&hourly={hourlyParams}&timezone=auto";
        }

        /// <summary>
        /// Deserializes the JSON response from the API.
        /// </summary>
        private OpenMeteoResponse DeserializeResponse(string json)
        {
            var serializer = new JavaScriptSerializer();
            var dict = serializer.Deserialize<dynamic>(json);

            var response = new OpenMeteoResponse
            {
                Latitude = Convert.ToDouble(dict["latitude"]),
                Longitude = Convert.ToDouble(dict["longitude"]),
                Hourly = new HourlyData()
            };

            if (dict.ContainsKey("hourly"))
            {
                var hourly = dict["hourly"];
                response.Hourly.Time = ConvertToList<string>(hourly["time"]);
                response.Hourly.Temperature_2m = ConvertToNullableDoubleList(hourly["temperature_2m"]);
                response.Hourly.Relative_Humidity_2m = ConvertToNullableDoubleList(hourly["relative_humidity_2m"]);
                response.Hourly.Precipitation = ConvertToNullableDoubleList(hourly["precipitation"]);
                response.Hourly.Wind_Speed_10m = ConvertToNullableDoubleList(hourly["wind_speed_10m"]);
                response.Hourly.Wind_Direction_10m = ConvertToNullableDoubleList(hourly["wind_direction_10m"]);
                response.Hourly.Pressure_Msl = ConvertToNullableDoubleList(hourly["pressure_msl"]);
                response.Hourly.Cloud_Cover = ConvertToNullableDoubleList(hourly["cloud_cover"]);
            }

            return response;
        }

        /// <summary>
        /// Converts a dynamic array to a strongly-typed list.
        /// </summary>
        private System.Collections.Generic.List<T> ConvertToList<T>(dynamic array)
        {
            var list = new System.Collections.Generic.List<T>();
            if (array != null)
            {
                foreach (var item in array)
                {
                    list.Add((T)item);
                }
            }
            return list;
        }

        /// <summary>
        /// Converts a dynamic array to a list of nullable doubles.
        /// </summary>
        private System.Collections.Generic.List<double?> ConvertToNullableDoubleList(dynamic array)
        {
            var list = new System.Collections.Generic.List<double?>();
            if (array != null)
            {
                foreach (var item in array)
                {
                    if (item == null)
                        list.Add(null);
                    else
                        list.Add(Convert.ToDouble(item));
                }
            }
            return list;
        }

        /// <summary>
        /// Maps the API response to a WeatherData object for the specified date/time.
        /// </summary>
        private WeatherData MapToWeatherData(OpenMeteoResponse response, WeatherLocation location, DateTime dateTime)
        {
            var weatherData = new WeatherData
            {
                DateTime = dateTime,
                Location = location
            };

            if (response.Hourly?.Time != null)
            {
                var targetTime = dateTime.ToString("yyyy-MM-ddTHH:00");
                var index = response.Hourly.Time.FindIndex(t => t.StartsWith(targetTime));

                if (index >= 0)
                {
                    weatherData.TemperatureCelsius = GetValueAtIndex(response.Hourly.Temperature_2m, index);
                    weatherData.Humidity = GetValueAtIndex(response.Hourly.Relative_Humidity_2m, index);
                    weatherData.Precipitation = GetValueAtIndex(response.Hourly.Precipitation, index);
                    weatherData.WindSpeed = GetValueAtIndex(response.Hourly.Wind_Speed_10m, index);
                    weatherData.WindDirection = GetValueAtIndex(response.Hourly.Wind_Direction_10m, index);
                    weatherData.Pressure = GetValueAtIndex(response.Hourly.Pressure_Msl, index);
                    weatherData.CloudCover = GetValueAtIndex(response.Hourly.Cloud_Cover, index);
                    weatherData.Description = GenerateDescription(weatherData);
                }
            }

            return weatherData;
        }

        /// <summary>
        /// Safely gets a value from a list at the specified index.
        /// </summary>
        private double? GetValueAtIndex(System.Collections.Generic.List<double?> list, int index)
        {
            if (list != null && index >= 0 && index < list.Count)
                return list[index];
            return null;
        }

        /// <summary>
        /// Generates a human-readable weather description based on the weather data.
        /// </summary>
        private string GenerateDescription(WeatherData data)
        {
            if (data.CloudCover.HasValue && data.Precipitation.HasValue && data.TemperatureCelsius.HasValue)
            {
                if (data.Precipitation.Value > 0)
                    return "Rainy";
                if (data.CloudCover.Value > 75)
                    return "Cloudy";
                if (data.CloudCover.Value > 25)
                    return "Partly Cloudy";
                return "Clear";
            }
            return "Unknown";
        }

        /// <summary>
        /// Disposes the HttpClient if it was created by this instance.
        /// </summary>
        public void Dispose()
        {
            if (_disposeHttpClient)
            {
                _httpClient?.Dispose();
            }
        }
    }
}
