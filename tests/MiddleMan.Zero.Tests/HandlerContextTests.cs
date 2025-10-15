using MiddleMan.Zero.Abstractions;
using Xunit;

namespace MiddleMan.Zero.Tests
{
    public class HandlerContextTests
    {
        [Fact]
        public void IsRequestValid_ReturnsTrue_WhenNoInvalidRequestMessages()
        {
            // Arrange
            var context = new HandlerContext();
            var regularMessage = new DebugMessage();

            // Act
            context.Log(regularMessage);

            // Assert
            Assert.True(context.IsRequestValid);
            Assert.True(context.IsSuccessful);
        }

        [Fact]
        public void IsRequestValid_ReturnsFalse_WhenInvalidRequestMessageExists()
        {
            // Arrange
            var context = new HandlerContext();
            var invalidMessage = new InvalidRequestMessage("Invalid request");

            // Act
            context.Log(invalidMessage);

            // Assert
            Assert.False(context.IsRequestValid);
            Assert.False(context.IsSuccessful);
        }

        [Fact]
        public void IsRequestValid_ReturnsFalse_WhenMultipleMessagesIncludingInvalidRequestMessage()
        {
            // Arrange
            var context = new HandlerContext();
            var regularMessage = new DebugMessage();
            var invalidMessage = new InvalidRequestMessage("Invalid request");

            // Act
            context.Log(regularMessage);
            context.Log(invalidMessage);

            // Assert
            Assert.False(context.IsRequestValid);
            Assert.False(context.IsSuccessful);
        }
    }
}