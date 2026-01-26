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

        /// <summary>
        /// Gets the wind direction as a compass value (N, NE, E, SE, S, SW, W, NW).
        /// </summary>
        public string WindDirectionCompass
        {
            get
            {
                if (!WindDirection.HasValue)
                    return null;

                return DegreesToCompass(WindDirection.Value);
            }
        }

        /// <summary>
        /// Gets or sets the sunshine duration in hours for the day.
        /// </summary>
        public double? SunshineDurationHours { get; set; }

        /// <summary>
        /// Gets or sets the sunshine duration in seconds for the day.
        /// </summary>
        public double? SunshineDurationSeconds { get; set; }

        /// <summary>
        /// Converts degrees to compass direction.
        /// </summary>
        /// <param name="degrees">The direction in degrees (0-360).</param>
        /// <returns>Compass direction (N, NE, E, SE, S, SW, W, NW).</returns>
        private static string DegreesToCompass(double degrees)
        {
            // Normalize to 0-360 range
            degrees = degrees % 360;
            if (degrees < 0)
                degrees += 360;

            // 8-point compass: N, NE, E, SE, S, SW, W, NW
            // Each direction covers 45 degrees, centered on the cardinal/intercardinal direction
            var compassPoints = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            var index = (int)Math.Round(degrees / 45.0) % 8;

            return compassPoints[index];
        }

    }
}
