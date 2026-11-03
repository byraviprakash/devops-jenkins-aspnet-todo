using devops_jenkins_aspnet_todo.api;

namespace devops_jenkins_aspnet_todo.test;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 211)]
    [InlineData(-20, -3)]
    [InlineData(37, 98)]
    public void TemperatureF_IsCalculatedFromTemperatureC(int celsius, int expectedFahrenheit)
    {
        var forecast = new WeatherForecast { TemperatureC = celsius };

        Assert.Equal(expectedFahrenheit, forecast.TemperatureF);
    }

    [Fact]
    public void Summary_DefaultsToNull()
    {
        var forecast = new WeatherForecast();

        Assert.Null(forecast.Summary);
    }
}
