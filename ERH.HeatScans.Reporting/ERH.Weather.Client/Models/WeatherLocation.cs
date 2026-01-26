using System;

namespace ERH.Weather.Client.Models
{
    /// <summary>
    /// Represents a geographic location with latitude and longitude coordinates.
    /// </summary>
    public class WeatherLocation
    {
        /// <summary>
        /// Gets the latitude in decimal degrees (range: -90 to +90).
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Gets the longitude in decimal degrees (range: -180 to +180).
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Initializes a new instance of the WeatherLocation class.
        /// </summary>
        /// <param name="latitude">The latitude in decimal degrees.</param>
        /// <param name="longitude">The longitude in decimal degrees.</param>
        public WeatherLocation(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and +90 degrees.");

            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and +180 degrees.");

            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
