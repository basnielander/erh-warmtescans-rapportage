# Wind Direction Compass Conversion

## Overview
The `WeatherData` model now includes a `WindDirectionCompass` property that automatically converts wind direction degrees to 8-point compass notation, matching the format used in the reporting client.

## Compass Points
- **N** (Noord) - North: 337.5° - 22.5° (0°)
- **NE** (Noordoost) - Northeast: 22.5° - 67.5° (45°)
- **E** (Oost) - East: 67.5° - 112.5° (90°)
- **SE** (Zuidoost) - Southeast: 112.5° - 157.5° (135°)
- **S** (Zuid) - South: 157.5° - 202.5° (180°)
- **SW** (Zuidwest) - Southwest: 202.5° - 247.5° (225°)
- **W** (West) - West: 247.5° - 292.5° (270°)
- **NW** (Noordwest) - Northwest: 292.5° - 337.5° (315°)

## Usage Example

```csharp
var location = new WeatherLocation(52.0907, 5.1214);
var dateTime = new DateTime(2024, 1, 22, 14, 0, 0);

using (var client = new HistoricalWeatherClient())
{
    var weatherData = await client.GetHistoricalWeatherAsync(location, dateTime);
    
    // Access wind direction in degrees
    Console.WriteLine($"Wind Direction: {weatherData.WindDirection}°");
    
    // Access wind direction as compass value
    Console.WriteLine($"Wind Direction Compass: {weatherData.WindDirectionCompass}");
    
    // Example output:
    // Wind Direction: 245°
    // Wind Direction Compass: SW
}
```

## Conversion Table

| Degrees | Compass | Dutch Name |
|---------|---------|------------|
| 0° | N | Noord |
| 45° | NE | Noordoost |
| 90° | E | Oost |
| 135° | SE | Zuidoost |
| 180° | S | Zuid |
| 225° | SW | Zuidwest |
| 270° | W | West |
| 315° | NW | Noordwest |

## Implementation Details

The conversion uses the standard meteorological convention where:
- Wind direction indicates where the wind is **coming from**
- 0° = North
- 90° = East
- 180° = South
- 270° = West

The algorithm divides the 360° circle into 8 equal sectors of 45° each, centered on the cardinal and intercardinal directions.
