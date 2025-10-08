namespace RHF_Foundation.Models
{
    public class Forecast
    {
        // Example: 72.5
        public double Temperature { get; set; }

        // Example: "°F" or "°C"
        public string TemperatureUnit { get; set; }

        // Example: 40 (meaning 40%)
        public int ProbabilityOfPrecipitation { get; set; }

        // Example: "15 mph" or "24 km/h"
        public string WindSpeed { get; set; }

        // Example: "NW", "SSE", "East"
        public string WindDirection { get; set; }

        // Optional: quick display helper property
        public string Summary =>
            $"{Temperature}{TemperatureUnit} | {WindSpeed} {WindDirection} | {ProbabilityOfPrecipitation}% chance of rain";
    }
}
