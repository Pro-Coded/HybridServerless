using HybridServerless.API.Features.Weather.Models;

namespace HybridServerless.API.Features.Weather;

public class WeatherService : IWeatherService
{
    private readonly string[] _summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherForecast[] GetWeather() => Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast
                        (
                            DateTime.Now.AddDays(index),
                            Random.Shared.Next(-20, 55),
                            _summaries[Random.Shared.Next(_summaries.Length)]
                        ))
                        .ToArray();
}
