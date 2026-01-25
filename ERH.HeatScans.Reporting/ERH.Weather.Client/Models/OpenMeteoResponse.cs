using System.Collections.Generic;

namespace ERH.Weather.Client.Models
{
    /// <summary>
    /// Represents the API response from Open-Meteo Archive API.
    /// </summary>
    internal class OpenMeteoResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public HourlyData Hourly { get; set; }
        public DailyData Daily { get; set; }
    }

    /// <summary>
    /// Represents hourly weather data from the API response.
    /// </summary>
    internal class HourlyData
    {
        public List<string> Time { get; set; }
        public List<double?> Temperature_2m { get; set; }
        public List<double?> Wind_Speed_10m { get; set; }
        public List<double?> Wind_Direction_10m { get; set; }
    }

    /// <summary>
    /// Represents daily weather data from the API response.
    /// </summary>
    internal class DailyData
    {
        public List<string> Time { get; set; }
        public List<double?> Sunshine_Duration { get; set; }
    }
}
