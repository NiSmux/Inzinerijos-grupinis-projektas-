using Xunit;
using MyBackend.Controllers;

namespace MyApp.Tests
{
    public class WeatherForecastTests
    {
        [Fact]
        public void TemperatureF_IsCalculatedCorrectly()
        {
            // Arrange
            var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 0, "Cold");

            // Act
            var tempF = forecast.TemperatureF;

            // Assert
            Assert.Equal(32, tempF);
        }
    }
}
