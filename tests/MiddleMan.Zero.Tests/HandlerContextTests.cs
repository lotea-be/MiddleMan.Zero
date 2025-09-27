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
            var regularMessage = new TestMessage();

            // Act
            context.LogMessage(regularMessage);

            // Assert
            Assert.True(context.IsRequestValid);
        }

        [Fact]
        public void IsRequestValid_ReturnsFalse_WhenInvalidRequestMessageExists()
        {
            // Arrange
            var context = new HandlerContext();
            var invalidMessage = new InvalidRequestMessage("Invalid request");

            // Act
            context.LogMessage(invalidMessage);

            // Assert
            Assert.False(context.IsRequestValid);
        }

        [Fact]
        public void IsRequestValid_ReturnsFalse_WhenMultipleMessagesIncludingInvalidRequestMessage()
        {
            // Arrange
            var context = new HandlerContext();
            var regularMessage = new TestMessage();
            var invalidMessage = new InvalidRequestMessage("Invalid request");

            // Act
            context.LogMessage(regularMessage);
            context.LogMessage(invalidMessage);

            // Assert
            Assert.False(context.IsRequestValid);
        }

        [Fact]
        public async Task HandlerBase_SkipsProcessing_WhenRequestIsInvalid()
        {
            // Arrange
            var handler = new TestHandlerWithInvalidation();
            var request = new TestRequest();

            // Act
            await handler.HandleAsync(request);

            // Assert
            Assert.False(handler.HandlerExecuted);
        }

        private class TestMessage : MessageBase { }

        private class TestRequest { }

        private class TestHandlerWithInvalidation : HandlerBase<TestRequest>
        {
            public bool HandlerExecuted { get; private set; }

            protected override ValueTask ValidateAsync(TestRequest request, HandlerContext context)
            {
                // Add an invalid request message
                context.LogMessage(new InvalidRequestMessage("Test validation failure"));
                return ValueTask.CompletedTask;
            }

            protected override ValueTask HandleAsync(TestRequest request, HandlerContext context)
            {
                // This should not be called if validation fails
                HandlerExecuted = true;
                return ValueTask.CompletedTask;
            }
        }
    }
}