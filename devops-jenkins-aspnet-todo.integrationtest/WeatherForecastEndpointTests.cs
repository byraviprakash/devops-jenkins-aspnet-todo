using System.Net;
using System.Net.Http.Json;
using devops_jenkins_aspnet_todo.api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace devops_jenkins_aspnet_todo.integrationtest;

public class WeatherForecastEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_WeatherForecast_ReturnsSuccessAndJson()
    {
        var response = await _client.GetAsync("/WeatherForecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_WeatherForecast_ReturnsFiveItems()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");

        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Length);
        Assert.All(forecasts, f =>
        {
            Assert.InRange(f.TemperatureC, -20, 54);
            Assert.False(string.IsNullOrWhiteSpace(f.Summary));
        });
    }

    [Fact]
    public async Task Get_UnknownRoute_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
