using System.Threading.Tasks;

namespace Kitchensink
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var location = new WeatherLocation(52.02081, 5.0946);
            var dateTime = new DateTime(2024, 1, 22, 21, 0, 0);

            using (var client = new HistoricalWeatherClient())
            {
                var weatherData = await client.GetHistoricalWeatherAsync(location, dateTime);

                Console.WriteLine($"DateTime: {weatherData.DateTime}");
                Console.WriteLine($"Temperature: {weatherData.TemperatureCelsius}°C");
                Console.WriteLine($"Humidity: {weatherData.Humidity}%");
                Console.WriteLine($"Precipitation: {weatherData.Precipitation} mm");
                Console.WriteLine($"Wind Speed: {weatherData.WindSpeed} km/h");
                Console.WriteLine($"Description: {weatherData.Description}");
            }
        }
    }
}
