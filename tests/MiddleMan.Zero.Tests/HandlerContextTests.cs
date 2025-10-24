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
            context.ShouldSatisfyAllConditions(
                () => context.IsRequestValid.ShouldBeTrue(),
                () => context.IsSuccessful.ShouldBeTrue()
            );
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
            context.ShouldSatisfyAllConditions(
                () => context.IsRequestValid.ShouldBeFalse(),
                () => context.IsSuccessful.ShouldBeFalse()
            );
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
            context.ShouldSatisfyAllConditions(
                () => context.IsRequestValid.ShouldBeFalse(),
                () => context.IsSuccessful.ShouldBeFalse()
            );
        }
    }
}