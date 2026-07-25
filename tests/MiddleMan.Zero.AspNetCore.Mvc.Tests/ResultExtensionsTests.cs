using Microsoft.AspNetCore.Mvc;

namespace MiddleMan.Zero.AspNetCore.Mvc.Tests;

public class ResultExtensionsTests
{
    private const string ProblemJsonContentType = "application/problem+json";

    #region ToActionResult (non-generic Result)

    [Fact]
    public void ToActionResult_ReturnsOkResult_WhenResultIsSuccessful()
    {
        // Arrange
        var result = new Result(ResultStatus.Successful, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public void ToActionResult_ReturnsEnvelope404_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result(ResultStatus.NotFound, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 404, "Not Found");
    }

    [Fact]
    public void ToActionResult_ReturnsEnvelope400_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid input", "validation_error") };
        var result = new Result(ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 400, "Bad Request");
    }

    [Fact]
    public void ToActionResult_ReturnsEnvelope500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result(ResultStatus.Failure, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 500, "Internal Server Error");
    }

    [Fact]
    public void ToActionResult_ReturnsEnvelope403_AndNotForbidResult_WhenResultIsForbidden()
    {
        // Arrange
        var messages = new MessageBase[] { new ForbiddenMessage("Access denied", "access_denied") };
        var result = new Result(ResultStatus.Forbidden, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldNotBeOfType<ForbidResult>();
        AssertEnvelope(actionResult, 403, "Forbidden");
    }

    [Fact]
    public void ToActionResult_ReturnsEnvelope409_WhenResultIsConflict()
    {
        // Arrange
        var messages = new MessageBase[] { new ConflictMessage("Already exists") };
        var result = new Result(ResultStatus.Conflict, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 409, "Conflict");
    }

    [Fact]
    public void ToActionResult_ReturnsEnvelope500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result(ResultStatus.Undefined, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 500, "Internal Server Error");
    }

    #endregion

    #region ToActionResult (generic Result<TResponse>)

    [Fact]
    public void ToActionResult_Generic_ReturnsOkObjectResult_WhenResultIsSuccessfulWithResponse()
    {
        // Arrange
        var response = new TestResponse { Id = 1, Name = "Test" };
        var result = new Result<TestResponse>(response, ResultStatus.Successful, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.ShouldSatisfyAllConditions(
            () => okResult.StatusCode.ShouldBe(200),
            () => okResult.Value.ShouldBe(response)
        );
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSuccessfulResultHasNullResponse()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new Result<TestResponse>(null, ResultStatus.Successful, []));

        exception.ParamName.ShouldBe("response");
        exception.Message.ShouldContain("Response cannot be null when ResultStatus is Successful");
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsEnvelope404_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 404, "Not Found");
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsEnvelope400_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 400, "Bad Request");
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsEnvelope500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 500, "Internal Server Error");
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsEnvelope403_AndNotForbidResult_WhenResultIsForbidden()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Forbidden, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldNotBeOfType<ForbidResult>();
        AssertEnvelope(actionResult, 403, "Forbidden");
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsEnvelope409_WhenResultIsConflict()
    {
        // Arrange
        var messages = new MessageBase[] { new ConflictMessage("Already exists") };
        var result = new Result<TestResponse>(null, ResultStatus.Conflict, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 409, "Conflict");
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsEnvelope500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Undefined, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        AssertEnvelope(actionResult, 500, "Internal Server Error");
    }

    #endregion

    #region ToTypedActionResult (ActionResult<TResponse>)

    [Fact]
    public void ToTypedActionResult_ReturnsResponse_WhenResultIsSuccessfulWithResponse()
    {
        // Arrange
        var response = new TestResponse { Id = 1, Name = "Test" };
        var result = new Result<TestResponse>(response, ResultStatus.Successful, []);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        actionResult.Result.ShouldBeNull();
        actionResult.Value.ShouldBe(response);
    }

    [Fact]
    public void ToTypedActionResult_ReturnsEnvelope404_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        AssertEnvelope(actionResult.Result!, 404, "Not Found");
    }

    [Fact]
    public void ToTypedActionResult_ReturnsEnvelope400_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        AssertEnvelope(actionResult.Result!, 400, "Bad Request");
    }

    [Fact]
    public void ToTypedActionResult_ReturnsEnvelope500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        AssertEnvelope(actionResult.Result!, 500, "Internal Server Error");
    }

    [Fact]
    public void ToTypedActionResult_ReturnsEnvelope403_AndNotForbidResult_WhenResultIsForbidden()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Forbidden, []);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        actionResult.Result.ShouldNotBeOfType<ForbidResult>();
        AssertEnvelope(actionResult.Result!, 403, "Forbidden");
    }

    [Fact]
    public void ToTypedActionResult_ReturnsEnvelope409_WhenResultIsConflict()
    {
        // Arrange
        var messages = new MessageBase[] { new ConflictMessage("Already exists") };
        var result = new Result<TestResponse>(null, ResultStatus.Conflict, messages);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        AssertEnvelope(actionResult.Result!, 409, "Conflict");
    }

    [Fact]
    public void ToTypedActionResult_ReturnsEnvelope500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result<TestResponse>(null, ResultStatus.Undefined, []);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        AssertEnvelope(actionResult.Result!, 500, "Internal Server Error");
    }

    #endregion

    #region Messages in Response

    [Fact]
    public void ToActionResult_ProjectsMessagesToErrorMessage_WhenResultHasMessages()
    {
        // Arrange
        var messages = new MessageBase[]
        {
            new InvalidRequestMessage("Error 1", "code_1"),
            new InvalidRequestMessage("Error 2", "code_2"),
        };
        var result = new Result(ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var body = AssertEnvelope(actionResult, 400, "Bad Request");
        body.Messages.Count.ShouldBe(2);
        body.Messages[0].Message.ShouldBe("Error 1");
        body.Messages[0].Code.ShouldBe("code_1");
        body.Messages[1].Message.ShouldBe("Error 2");
        body.Messages[1].Code.ShouldBe("code_2");
        // Detail is the joined message text when messages are present.
        body.Detail.ShouldBe("Error 1; Error 2");
    }

    [Fact]
    public void ToActionResult_UsesDefaultDetail_WhenResultHasNoMessages()
    {
        // Arrange
        var result = new Result(ResultStatus.Forbidden, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var body = AssertEnvelope(actionResult, 403, "Forbidden");
        body.Messages.ShouldBeEmpty();
        body.Detail.ShouldBe("Access denied.");
    }

    #endregion

    private static ProblemResponse AssertEnvelope(IActionResult actionResult, int expectedStatus, string expectedTitle)
    {
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.ShouldSatisfyAllConditions(
            () => objectResult.StatusCode.ShouldBe(expectedStatus),
            () => objectResult.ContentTypes.ShouldContain(ProblemJsonContentType)
        );

        var body = objectResult.Value.ShouldBeOfType<ProblemResponse>();
        body.ShouldSatisfyAllConditions(
            () => body.Status.ShouldBe(expectedStatus),
            () => body.Title.ShouldBe(expectedTitle),
            () => body.Type.ShouldNotBeNullOrEmpty(),
            () => body.Detail.ShouldNotBeNullOrEmpty(),
            () => body.Messages.ShouldNotBeNull()
        );
        return body;
    }

    // Test helper class
    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
