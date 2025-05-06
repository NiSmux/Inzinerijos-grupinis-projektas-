using Xunit;
using MyBackend.Controllers;
using System.Linq;

namespace MyApp.Tests
{
    public class WeatherControllerTests
    {
        [Fact]
        public void Get_ReturnsFiveForecasts_WithNonNullSummaries()
        {
            // Arrange
            var controller = new WeatherController();

            // Act
            var result = controller.Get().ToArray();

            // Assert
            Assert.Equal(5, result.Length);
            Assert.All(result, item =>
            {
                Assert.False(string.IsNullOrWhiteSpace(item.Summary));
            });
        }
    }
}
