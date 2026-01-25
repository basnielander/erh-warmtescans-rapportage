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
                // Get hourly weather data
                Console.WriteLine("=== Historical Weather Data ===");
                var weatherData = await client.GetHistoricalWeatherAsync(location, dateTime);

                Console.WriteLine($"DateTime: {weatherData.DateTime}");
                Console.WriteLine($"Temperature: {weatherData.TemperatureCelsius}°C");
                Console.WriteLine($"Wind Direction: {weatherData.WindDirection}°");
                Console.WriteLine($"Wind Speed: {weatherData.WindSpeed} km/h");

                Console.WriteLine();

                // Get sunshine hours for the date
                Console.WriteLine("=== Sunshine Duration Data ===");
                var sunshineData = await client.GetSunshineHoursAsync(location, dateTime.Date);

                Console.WriteLine($"Date: {sunshineData.Date:yyyy-MM-dd}");
                Console.WriteLine($"Sunshine Duration: {sunshineData.SunshineDurationHours:F2} hours");
                Console.WriteLine($"Sunshine Duration: {sunshineData.SunshineDurationSeconds:F0} seconds");
            }
        }
    }
}
