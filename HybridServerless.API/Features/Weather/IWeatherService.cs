using HybridServerless.API.Features.Weather.Models;

namespace HybridServerless.API.Features.Weather;
public interface IWeatherService
{
    WeatherForecast[] GetWeather();
}