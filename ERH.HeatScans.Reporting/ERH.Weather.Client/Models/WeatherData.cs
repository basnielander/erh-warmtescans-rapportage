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
        /// Gets or sets the wind speed in km/h.
        /// </summary>
        public double? WindSpeed { get; set; }

        /// <summary>
        /// Gets or sets the wind direction in degrees (0-360).
        /// </summary>
        public double? WindDirection { get; set; }

    }
}
