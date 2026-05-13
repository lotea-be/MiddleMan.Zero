using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MiddleMan.Zero.AspNetCore.Http.Tests;

public class ResultExtensionsTests
{
    #region ToResult (non-generic ResultBase)

    [Fact]
    public void ToResult_ReturnsOk_WhenResultIsSuccessful()
    {
        // Arrange
        var result = new Result(ResultStatus.Successful, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        iResult.ShouldBeOfType<Ok>();
    }

    [Fact]
    public void ToResult_ReturnsNotFoundWithStatusCode404_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result(ResultStatus.NotFound, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var statusResult = iResult.ShouldBeAssignableTo<IStatusCodeHttpResult>();
        statusResult!.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void ToResult_ReturnsBadRequestWithStatusCode400_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid input") };
        var result = new Result(ResultStatus.Invalid, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var statusResult = iResult.ShouldBeAssignableTo<IStatusCodeHttpResult>();
        statusResult!.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void ToResult_ReturnsProblemWithStatusCode500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result(ResultStatus.Failure, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var problemResult = iResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(500);
    }

    [Fact]
    public void ToResult_ReturnsForbid_WhenResultIsForbidden()
    {
        // Arrange
        var result = new Result(ResultStatus.Forbidden, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        iResult.ShouldBeOfType<ForbidHttpResult>();
    }

    [Fact]
    public void ToResult_ReturnsConflictWithStatusCode409_WhenResultIsConflict()
    {
        // Arrange
        var messages = new MessageBase[] { new ConflictMessage("Already exists") };
        var result = new Result(ResultStatus.Conflict, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var statusResult = iResult.ShouldBeAssignableTo<IStatusCodeHttpResult>();
        statusResult!.StatusCode.ShouldBe(409);
    }

    [Fact]
    public void ToResult_ReturnsProblemWithStatusCode500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result(ResultStatus.Undefined, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        var problemResult = iResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(500);
    }

    [Fact]
    public void ToResult_IncludesMessagesInNotFound_WhenResultHasMessages()
    {
        // Arrange
        var message = new NotFoundMessage();
        var messages = new MessageBase[] { message };
        var result = new Result(ResultStatus.NotFound, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var valueResult = iResult.ShouldBeAssignableTo<IValueHttpResult>();
        var messagesProperty = valueResult!.Value!.GetType().GetProperty("messages");
        messagesProperty.ShouldNotBeNull();
        var messagesValue = messagesProperty.GetValue(valueResult.Value) as MessageBase[];
        messagesValue.ShouldNotBeNull();
        messagesValue.Length.ShouldBe(1);
    }

    [Fact]
    public void ToResult_IncludesMessagesInBadRequest_WhenResultHasMessages()
    {
        // Arrange
        var message1 = new InvalidRequestMessage("Error 1");
        var message2 = new InvalidRequestMessage("Error 2");
        var messages = new MessageBase[] { message1, message2 };
        var result = new Result(ResultStatus.Invalid, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var valueResult = iResult.ShouldBeAssignableTo<IValueHttpResult>();
        var messagesProperty = valueResult!.Value!.GetType().GetProperty("messages");
        messagesProperty.ShouldNotBeNull();
        var messagesValue = messagesProperty.GetValue(valueResult.Value) as MessageBase[];
        messagesValue.ShouldNotBeNull();
        messagesValue.Length.ShouldBe(2);
    }

    #endregion

    #region ToResult<TResponse> (generic ResultBase<TResponse>)

    [Fact]
    public void ToResult_Generic_ReturnsOkWithResponse_WhenResultIsSuccessful()
    {
        // Arrange
        var response = new TestResponse { Id = 1, Name = "Test" };
        var result = new Result<TestResponse>(response, ResultStatus.Successful, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        var okResult = iResult.ShouldBeOfType<Ok<TestResponse>>();
        okResult.Value.ShouldBe(response);
    }

    [Fact]
    public void ToResult_Generic_ReturnsNotFoundWithStatusCode404_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var statusResult = iResult.ShouldBeAssignableTo<IStatusCodeHttpResult>();
        statusResult!.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void ToResult_Generic_ReturnsBadRequestWithStatusCode400_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var statusResult = iResult.ShouldBeAssignableTo<IStatusCodeHttpResult>();
        statusResult!.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void ToResult_Generic_ReturnsProblemWithStatusCode500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var problemResult = iResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(500);
    }

    [Fact]
    public void ToResult_Generic_ReturnsForbid_WhenResultIsForbidden()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Forbidden, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        iResult.ShouldBeOfType<ForbidHttpResult>();
    }

    [Fact]
    public void ToResult_Generic_ReturnsConflictWithStatusCode409_WhenResultIsConflict()
    {
        // Arrange
        var messages = new MessageBase[] { new ConflictMessage("Already exists") };
        var result = new Result<TestResponse>(null, ResultStatus.Conflict, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var statusResult = iResult.ShouldBeAssignableTo<IStatusCodeHttpResult>();
        statusResult!.StatusCode.ShouldBe(409);
    }

    [Fact]
    public void ToResult_Generic_ReturnsProblemWithStatusCode500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Undefined, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        var problemResult = iResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(500);
    }

    #endregion

    // Test helper class
    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
