using System;

namespace ERH.Weather.Client.Models
{
    /// <summary>
    /// Represents historical weather data for a specific date, time, and location.
    /// </summary>
    public class WeatherData
    {
        /// <summary>
        /// Gets or sets the date and time for which the weather data applies.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the geographic location.
        /// </summary>
        public WeatherLocation Location { get; set; }

        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        public double? TemperatureCelsius { get; set; }

        /// <summary>
        /// Gets or sets the relative humidity as a percentage (0-100).
        /// </summary>
        public double? Humidity { get; set; }

        /// <summary>
        /// Gets or sets the precipitation amount in millimeters.
        /// </summary>
        public double? Precipitation { get; set; }

        /// <summary>
        /// Gets or sets the wind speed in km/h.
        /// </summary>
        public double? WindSpeed { get; set; }

        /// <summary>
        /// Gets or sets the wind direction in degrees (0-360).
        /// </summary>
        public double? WindDirection { get; set; }

        /// <summary>
        /// Gets or sets the atmospheric pressure at sea level in hPa.
        /// </summary>
        public double? Pressure { get; set; }

        /// <summary>
        /// Gets or sets the cloud cover as a percentage (0-100).
        /// </summary>
        public double? CloudCover { get; set; }

        /// <summary>
        /// Gets or sets a description of the weather conditions.
        /// </summary>
        public string Description { get; set; }
    }
}
