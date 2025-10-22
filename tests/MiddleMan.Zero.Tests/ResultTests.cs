using MiddleMan.Zero.Abstractions;

namespace MiddleMan.Zero.Tests;

public class ResultTests
{
    #region Non-Generic Result

    [Fact]
    public void Result_CanBeCreated_WithSuccessfulStatus()
    {
        // Act
        var result = new Result(ResultStatus.Successful, []);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful),
            () => result.Messages.ShouldBeEmpty()
        );
    }

    [Fact]
    public void Result_CanBeCreated_WithFailureStatus()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };

        // Act
        var result = new Result(ResultStatus.Failure, messages);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Failure),
            () => result.Messages.Length.ShouldBe(1)
        );
    }

    #endregion

    #region Generic Result<TResponse>

    [Fact]
    public void GenericResult_CanBeCreated_WithSuccessfulStatusAndResponse()
    {
        // Arrange
        var response = new TestResponse { Id = 1, Name = "Test" };

        // Act
        var result = new Result<TestResponse>(response, ResultStatus.Successful, []);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful),
            () => result.Response.ShouldBe(response),
            () => result.Messages.ShouldBeEmpty()
        );
    }

    [Fact]
    public void GenericResult_ThrowsArgumentNullException_WhenSuccessfulWithNullResponse()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new Result<TestResponse>(null, ResultStatus.Successful, []));

        exception.ShouldSatisfyAllConditions(
            () => exception.ParamName.ShouldBe("response"),
            () => exception.Message.ShouldContain("Response cannot be null when ResultStatus is Successful")
        );
    }

    [Fact]
    public void GenericResult_CanBeCreated_WithNotFoundStatusAndNullResponse()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };

        // Act
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.NotFound),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.Length.ShouldBe(1)
        );
    }

    [Fact]
    public void GenericResult_CanBeCreated_WithInvalidStatusAndNullResponse()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };

        // Act
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.Length.ShouldBe(1)
        );
    }

    [Fact]
    public void GenericResult_CanBeCreated_WithFailureStatusAndNullResponse()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };

        // Act
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Failure),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.Length.ShouldBe(1)
        );
    }

    [Fact]
    public void GenericResult_CanBeCreated_WithUndefinedStatusAndNullResponse()
    {
        // Act
        var result = new Result<TestResponse>(null, ResultStatus.Undefined, []);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Undefined),
            () => result.Response.ShouldBeNull()
        );
    }

    #endregion

    // Test helper class
    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
