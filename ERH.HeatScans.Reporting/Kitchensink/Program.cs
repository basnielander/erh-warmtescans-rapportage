using ERH.Weather.Client;
using ERH.Weather.Client.Models;
using System;
using System.Threading.Tasks;

namespace Kitchensink
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var location = new WeatherLocation(52.02081, 5.0946);
            var dateTime = new DateTime(2026, 1, 22, 21, 0, 0);

            using (var client = new HistoricalWeatherClient())
            {
                // Get all weather data including sunshine hours in a single API call
                Console.WriteLine("=== Historical Weather Data (Combined API Call) ===");
                var weatherData = await client.GetHistoricalWeatherAsync(location, dateTime);

                Console.WriteLine($"DateTime: {weatherData.DateTime}");
                Console.WriteLine($"Temperature: {weatherData.TemperatureCelsius}°C");
                Console.WriteLine($"Wind Direction: {weatherData.WindDirection}° ({weatherData.WindDirectionCompass})");
                Console.WriteLine($"Wind Speed: {weatherData.WindSpeed} km/h");
                Console.WriteLine($"Sunshine Duration: {weatherData.SunshineDurationHours:F2} hours");
                Console.WriteLine($"Sunshine Duration: {weatherData.SunshineDurationSeconds:F0} seconds");
            }
        }
    }
}
