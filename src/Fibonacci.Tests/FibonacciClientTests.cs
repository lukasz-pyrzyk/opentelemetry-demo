using System.Threading.Tasks;
using Fibonacci.WebServiceClient;
using FluentAssertions;
using Xunit;

namespace Fibonacci.Tests;

public class FibonacciClientTests
{
    [Theory]
    [InlineData(6, 8)]
    public async Task GetsFibonacciValue(int value, int expected)
    {
        // Arrange
        var client = new FibonacciClient();

        try
        {
            // Act
            var calculatedValue = await client.Calculate(value);

            // Assert
            calculatedValue.Should().Be(expected);
        }
        finally
        {
            await client.DeleteResult(value);
        }
    }
}