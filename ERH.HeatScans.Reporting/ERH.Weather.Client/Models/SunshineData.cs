using System;

namespace ERH.Weather.Client.Models
{
    /// <summary>
    /// Represents sunshine duration data for a specific date and location.
    /// </summary>
    public class SunshineData
    {
        /// <summary>
        /// Gets or sets the date for which the sunshine data applies.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the geographic location.
        /// </summary>
        public WeatherLocation Location { get; set; }

        /// <summary>
        /// Gets or sets the sunshine duration in hours for the day.
        /// </summary>
        public double? SunshineDurationHours { get; set; }

        /// <summary>
        /// Gets or sets the sunshine duration in seconds for the day.
        /// </summary>
        public double? SunshineDurationSeconds { get; set; }
    }
}
