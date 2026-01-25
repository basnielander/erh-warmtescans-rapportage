using ERH.Weather.Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            {
                throw new ArgumentNullException(nameof(location));
            }

            var date = dateTime.Date;
            var url = BuildUrl(location, date, date);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = DeserializeResponse(json);

            return MapToWeatherData(apiResponse, location, dateTime);
        }

        /// <summary>
        /// Retrieves hours of sunshine for a specific location and date.
        /// </summary>
        /// <param name="location">The geographic location for which to retrieve sunshine data.</param>
        /// <param name="date">The date for which to retrieve sunshine hours.</param>
        /// <returns>A SunshineData object containing the sunshine duration information.</returns>
        public async Task<SunshineData> GetSunshineHoursAsync(WeatherLocation location, DateTime date)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            var targetDate = date.Date;
            var url = BuildSunshineUrl(location, targetDate, targetDate);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = DeserializeResponse(json);

            return MapToSunshineData(apiResponse, location, targetDate);
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

            var hourlyParams = "temperature_2m,wind_speed_10m,wind_direction_10m";

            return $"{BaseUrl}?latitude={latitude}&longitude={longitude}&start_date={start}&end_date={end}&hourly={hourlyParams}&timezone=Europe%2FBerlin";
        }

        /// <summary>
        /// Builds the API URL for sunshine duration requests.
        /// </summary>
        private string BuildSunshineUrl(WeatherLocation location, DateTime startDate, DateTime endDate)
        {
            var latitude = location.Latitude.ToString("F4", CultureInfo.InvariantCulture);
            var longitude = location.Longitude.ToString("F4", CultureInfo.InvariantCulture);
            var start = startDate.ToString("yyyy-MM-dd");
            var end = endDate.ToString("yyyy-MM-dd");

            var dailyParams = "sunshine_duration";

            return $"{BaseUrl}?latitude={latitude}&longitude={longitude}&start_date={start}&end_date={end}&daily={dailyParams}&timezone=auto";
        }

        /// <summary>
        /// Deserializes the JSON response from the API.
        /// </summary>
        private OpenMeteoResponse DeserializeResponse(string json)
        {
            var response = new OpenMeteoResponse
            {
                Latitude = ExtractDouble(json, "latitude"),
                Longitude = ExtractDouble(json, "longitude"),
                Hourly = new HourlyData(),
                Daily = new DailyData()
            };

            var hourlyMatch = Regex.Match(json, @"""hourly"":\s*\{([^}]+(?:\{[^}]*\}[^}]*)*)\}");
            if (hourlyMatch.Success)
            {
                var hourlyJson = hourlyMatch.Groups[1].Value;

                response.Hourly.Time = ExtractStringArray(hourlyJson, "time");
                response.Hourly.Temperature_2m = ExtractDoubleArray(hourlyJson, "temperature_2m");
                response.Hourly.Wind_Speed_10m = ExtractDoubleArray(hourlyJson, "wind_speed_10m");
                response.Hourly.Wind_Direction_10m = ExtractDoubleArray(hourlyJson, "wind_direction_10m");
            }

            var dailyMatch = Regex.Match(json, @"""daily"":\s*\{([^}]+(?:\{[^}]*\}[^}]*)*)\}");
            if (dailyMatch.Success)
            {
                var dailyJson = dailyMatch.Groups[1].Value;

                response.Daily.Time = ExtractStringArray(dailyJson, "time");
                response.Daily.Sunshine_Duration = ExtractDoubleArray(dailyJson, "sunshine_duration");
            }

            return response;
        }

        /// <summary>
        /// Extracts a double value from JSON.
        /// </summary>
        private double ExtractDouble(string json, string key)
        {
            var match = Regex.Match(json, $@"""{key}"":\s*(-?\d+\.?\d*)");
            if (match.Success)
            {
                return double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            }
            return 0;
        }

        /// <summary>
        /// Extracts a string array from JSON.
        /// </summary>
        private List<string> ExtractStringArray(string json, string key)
        {
            var list = new List<string>();
            var match = Regex.Match(json, $@"""{key}"":\s*\[(.*?)\]");
            if (match.Success)
            {
                var values = match.Groups[1].Value;
                var stringMatches = Regex.Matches(values, @"""([^""]+)""");
                foreach (Match m in stringMatches)
                {
                    list.Add(m.Groups[1].Value);
                }
            }
            return list;
        }

        /// <summary>
        /// Extracts a nullable double array from JSON.
        /// </summary>
        private List<double?> ExtractDoubleArray(string json, string key)
        {
            var list = new List<double?>();
            var match = Regex.Match(json, $@"""{key}"":\s*\[(.*?)\]");
            if (match.Success)
            {
                var values = match.Groups[1].Value;
                var numberMatches = Regex.Matches(values, @"(-?\d+\.?\d*|null)");
                foreach (Match m in numberMatches)
                {
                    var value = m.Groups[1].Value;
                    if (value == "null")
                    {
                        list.Add(null);
                    }
                    else
                    {
                        list.Add(double.Parse(value, CultureInfo.InvariantCulture));
                    }
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
                    weatherData.WindSpeed = GetValueAtIndex(response.Hourly.Wind_Speed_10m, index);
                    weatherData.WindDirection = GetValueAtIndex(response.Hourly.Wind_Direction_10m, index);
                }
            }

            return weatherData;
        }

        /// <summary>
        /// Maps the API response to a SunshineData object for the specified date.
        /// </summary>
        private SunshineData MapToSunshineData(OpenMeteoResponse response, WeatherLocation location, DateTime date)
        {
            var sunshineData = new SunshineData
            {
                Date = date,
                Location = location
            };

            if (response.Daily?.Time != null)
            {
                var targetDate = date.ToString("yyyy-MM-dd");
                var index = response.Daily.Time.FindIndex(t => t == targetDate);

                if (index >= 0)
                {
                    var durationSeconds = GetValueAtIndex(response.Daily.Sunshine_Duration, index);
                    sunshineData.SunshineDurationSeconds = durationSeconds;
                    sunshineData.SunshineDurationHours = durationSeconds.HasValue ? durationSeconds.Value / 3600.0 : (double?)null;
                }
            }

            return sunshineData;
        }

        /// <summary>
        /// Safely gets a value from a list at the specified index.
        /// </summary>
        private double? GetValueAtIndex(System.Collections.Generic.List<double?> list, int index)
        {
            if (list != null && index >= 0 && index < list.Count)
            {
                return list[index];
            }

            return null;
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
