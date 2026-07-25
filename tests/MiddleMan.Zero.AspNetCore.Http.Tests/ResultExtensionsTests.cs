using Microsoft.AspNetCore.Http.HttpResults;

namespace MiddleMan.Zero.AspNetCore.Http.Tests;

public class ResultExtensionsTests
{
    private const string ProblemJsonContentType = "application/problem+json";

    #region ToResult (non-generic ResultBase) -- success path

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

    #endregion

    #region ToResult (non-generic ResultBase) -- non-success envelope

    [Fact]
    public void ToResult_ReturnsJsonEnvelope400_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid input", "validation_error") };
        var result = new Result(ResultStatus.Invalid, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(400);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);

        var body = jsonResult.Value;
        body.ShouldNotBeNull();
        body.Status.ShouldBe(400);
        body.Type.ShouldNotBeNullOrEmpty();
        body.Title.ShouldNotBeNullOrEmpty();
        body.Detail.ShouldNotBeNullOrEmpty();
        body.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_ReturnsJsonEnvelope403_WhenResultIsForbidden()
    {
        // Arrange
        var messages = new MessageBase[] { new ForbiddenMessage("Access denied", "access_denied") };
        var result = new Result(ResultStatus.Forbidden, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(403);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);

        var body = jsonResult.Value;
        body.ShouldNotBeNull();
        body.Status.ShouldBe(403);
        body.Type.ShouldNotBeNullOrEmpty();
        body.Title.ShouldNotBeNullOrEmpty();
        body.Detail.ShouldNotBeNullOrEmpty();
        body.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_ReturnsJsonEnvelope403_WhenResultIsForbidden_NotForbidHttpResult()
    {
        // Arrange
        var result = new Result(ResultStatus.Forbidden, []);

        // Act
        var iResult = result.ToResult();

        // Assert -- Forbidden must NOT produce ForbidHttpResult; it must produce the JSON envelope
        iResult.ShouldNotBeOfType<ForbidHttpResult>();
        iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
    }

    [Fact]
    public void ToResult_ReturnsJsonEnvelope404_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage("Resource not found", "not_found") };
        var result = new Result(ResultStatus.NotFound, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(404);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);

        var body = jsonResult.Value;
        body.ShouldNotBeNull();
        body.Status.ShouldBe(404);
        body.Type.ShouldNotBeNullOrEmpty();
        body.Title.ShouldNotBeNullOrEmpty();
        body.Detail.ShouldNotBeNullOrEmpty();
        body.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_ReturnsJsonEnvelope409_WhenResultIsConflict()
    {
        // Arrange
        var messages = new MessageBase[] { new ConflictMessage("Already exists", "already_exists") };
        var result = new Result(ResultStatus.Conflict, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(409);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);

        var body = jsonResult.Value;
        body.ShouldNotBeNull();
        body.Status.ShouldBe(409);
        body.Type.ShouldNotBeNullOrEmpty();
        body.Title.ShouldNotBeNullOrEmpty();
        body.Detail.ShouldNotBeNullOrEmpty();
        body.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_ReturnsJsonEnvelope500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage("An error occurred", "server_error") };
        var result = new Result(ResultStatus.Failure, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(500);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);

        var body = jsonResult.Value;
        body.ShouldNotBeNull();
        body.Status.ShouldBe(500);
        body.Type.ShouldNotBeNullOrEmpty();
        body.Title.ShouldNotBeNullOrEmpty();
        body.Detail.ShouldNotBeNullOrEmpty();
        body.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_ReturnsJsonEnvelope500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result(ResultStatus.Undefined, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(500);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);
    }

    [Fact]
    public void ToResult_MessagesArePresentInEnvelope_WhenResultHasMessages()
    {
        // Arrange
        var message1 = new InvalidRequestMessage("Error 1", "err_1");
        var message2 = new InvalidRequestMessage("Error 2", "err_2");
        var result = new Result(ResultStatus.Invalid, [message1, message2]);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        var body = jsonResult.Value;
        body.ShouldNotBeNull();
        body.Messages.Count.ShouldBe(2);
        body.Messages[0].Message.ShouldBe("Error 1");
        body.Messages[0].Code.ShouldBe("err_1");
        body.Messages[1].Message.ShouldBe("Error 2");
        body.Messages[1].Code.ShouldBe("err_2");
    }

    [Fact]
    public void ToResult_DetailIsJoinedMessages_WhenResultHasMessages()
    {
        // Arrange
        var message1 = new InvalidRequestMessage("Field A is required");
        var message2 = new InvalidRequestMessage("Field B is too long");
        var result = new Result(ResultStatus.Invalid, [message1, message2]);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.Value!.Detail.ShouldContain("Field A is required");
        jsonResult.Value.Detail.ShouldContain("Field B is too long");
    }

    #endregion

    #region ToResult<TResponse> (generic ResultBase<TResponse>) -- success path

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

    #endregion

    #region ToResult<TResponse> (generic ResultBase<TResponse>) -- non-success envelope

    [Fact]
    public void ToResult_Generic_ReturnsJsonEnvelope400_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid", "validation_error") };
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(400);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);
        jsonResult.Value!.Status.ShouldBe(400);
        jsonResult.Value.Type.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Title.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_Generic_ReturnsJsonEnvelope403_WhenResultIsForbidden()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Forbidden, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(403);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);
        jsonResult.Value!.Status.ShouldBe(403);
    }

    [Fact]
    public void ToResult_Generic_ReturnsJsonEnvelope403_WhenResultIsForbidden_NotForbidHttpResult()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Forbidden, []);

        // Act
        var iResult = result.ToResult();

        // Assert -- Forbidden must NOT produce ForbidHttpResult; it must produce the JSON envelope
        iResult.ShouldNotBeOfType<ForbidHttpResult>();
        iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
    }

    [Fact]
    public void ToResult_Generic_ReturnsJsonEnvelope404_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(404);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);
        jsonResult.Value!.Status.ShouldBe(404);
        jsonResult.Value.Type.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Title.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_Generic_ReturnsJsonEnvelope409_WhenResultIsConflict()
    {
        // Arrange
        var messages = new MessageBase[] { new ConflictMessage("Already exists") };
        var result = new Result<TestResponse>(null, ResultStatus.Conflict, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(409);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);
        jsonResult.Value!.Status.ShouldBe(409);
        jsonResult.Value.Type.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Title.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_Generic_ReturnsJsonEnvelope500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(500);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);
        jsonResult.Value!.Status.ShouldBe(500);
        jsonResult.Value.Type.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Title.ShouldNotBeNullOrEmpty();
        jsonResult.Value.Messages.ShouldNotBeNull();
    }

    [Fact]
    public void ToResult_Generic_ReturnsJsonEnvelope500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Undefined, []);

        // Act
        var iResult = result.ToResult();

        // Assert
        var jsonResult = iResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();
        jsonResult.StatusCode.ShouldBe(500);
        jsonResult.ContentType.ShouldBe(ProblemJsonContentType);
    }

    #endregion

    // Test helper class
    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
