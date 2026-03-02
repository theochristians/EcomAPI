using Xunit;
using FluentAssertions;

namespace EComAPI.Application.Tests.SimpleTest
{
    public class SimpleTestExample
    {
        [Fact]
        public void SimpleTest_ShouldPass()
        {
            // Arrange
            var expected = "Hello World";

            // Act
            var actual = "Hello World";

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(5, 10, 15)]
        public void Add_ShouldReturnCorrectSum(int a, int b, int expected)
        {
            // Act
            var result = a + b;

            // Assert
            result.Should().Be(expected);
        }
    }
}