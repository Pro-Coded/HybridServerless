using HybridServerless.API.ModulesInfrastructure;

namespace HybridServerless.API.Features.Weather;

public class WeatherModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services)
    {
        //services.AddSingleton(new WeatherServiceConfig());
        _ = services.AddScoped<IWeatherService, WeatherService>();

        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        _ = endpoints.MapGet("/weatherforecast", (IWeatherService weatherService) => weatherService.GetWeather())
                .WithName("GetWeatherForecast");

        return endpoints;
    }
}