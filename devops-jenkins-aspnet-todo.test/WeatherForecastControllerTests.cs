using devops_jenkins_aspnet_todo.api.Controllers;

namespace devops_jenkins_aspnet_todo.test;

public class WeatherForecastControllerTests
{
    private static readonly string[] ValidSummaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly WeatherForecastController _controller = new();

    [Fact]
    public void Get_ReturnsFiveForecasts()
    {
        var result = _controller.Get().ToList();

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Get_ReturnsConsecutiveFutureDates()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var result = _controller.Get().ToList();

        for (var i = 0; i < result.Count; i++)
        {
            Assert.Equal(today.AddDays(i + 1), result[i].Date);
        }
    }

    [Fact]
    public void Get_ReturnsTemperaturesWithinExpectedRange()
    {
        var result = _controller.Get();

        Assert.All(result, f => Assert.InRange(f.TemperatureC, -20, 54));
    }

    [Fact]
    public void Get_ReturnsKnownSummaries()
    {
        var result = _controller.Get();

        Assert.All(result, f => Assert.Contains(f.Summary, ValidSummaries));
    }
}
