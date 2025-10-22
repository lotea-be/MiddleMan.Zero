namespace MiddleMan.Zero.AspNetCore.Mvc.Tests;

public class ResultExtensionsTests
{
    #region ToActionResult (non-generic Result)

    [Fact]
    public void ToActionResult_ReturnsOkResult_WhenResultIsSuccessful()
    {
        // Arrange
        var result = new Result(ResultStatus.Successful, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<OkResult>()
        );
    }

    [Fact]
    public void ToActionResult_ReturnsNotFoundObjectResult_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result(ResultStatus.NotFound, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<NotFoundObjectResult>()
        );

        var notFoundResult = (NotFoundObjectResult)actionResult;
        notFoundResult.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void ToActionResult_ReturnsBadRequestObjectResult_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid input") };
        var result = new Result(ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<BadRequestObjectResult>()
        );

        var badRequestResult = (BadRequestObjectResult)actionResult;
        badRequestResult.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void ToActionResult_ReturnsObjectResultWith500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result(ResultStatus.Failure, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<ObjectResult>()
        );

        var objectResult = (ObjectResult)actionResult;
        objectResult.StatusCode.ShouldBe(500);
    }

    [Fact]
    public void ToActionResult_ReturnsObjectResultWith500_WhenResultIsUndefined()
    {
        // Arrange
        var result = new Result(ResultStatus.Undefined, []);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<ObjectResult>()
        );

        var objectResult = (ObjectResult)actionResult;
        objectResult.StatusCode.ShouldBe(500);
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
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<OkObjectResult>()
        );

        var okResult = (OkObjectResult)actionResult;
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
    public void ToActionResult_Generic_ReturnsNotFoundObjectResult_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<NotFoundObjectResult>()
        );

        var notFoundResult = (NotFoundObjectResult)actionResult;
        notFoundResult.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsBadRequestObjectResult_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<BadRequestObjectResult>()
        );

        var badRequestResult = (BadRequestObjectResult)actionResult;
        badRequestResult.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void ToActionResult_Generic_ReturnsObjectResultWith500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldSatisfyAllConditions(
            () => actionResult.ShouldNotBeNull(),
            () => actionResult.ShouldBeOfType<ObjectResult>()
        );

        var objectResult = (ObjectResult)actionResult;
        objectResult.StatusCode.ShouldBe(500);
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
    public void ToTypedActionResult_ReturnsNotFoundObjectResult_WhenResultIsNotFound()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        actionResult.Result.ShouldBeOfType<NotFoundObjectResult>();

        var notFoundResult = (NotFoundObjectResult)actionResult.Result!;
        notFoundResult.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void ToTypedActionResult_ReturnsBadRequestObjectResult_WhenResultIsInvalid()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        actionResult.Result.ShouldBeOfType<BadRequestObjectResult>();

        var badRequestResult = (BadRequestObjectResult)actionResult.Result!;
        badRequestResult.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void ToTypedActionResult_ReturnsObjectResultWith500_WhenResultIsFailure()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);

        // Act
        var actionResult = result.ToTypedActionResult();

        // Assert
        actionResult.ShouldNotBeNull();
        actionResult.Result.ShouldBeOfType<ObjectResult>();

        var objectResult = (ObjectResult)actionResult.Result!;
        objectResult.StatusCode.ShouldBe(500);
    }

    #endregion

    #region Messages in Response

    [Fact]
    public void ToActionResult_IncludesMessages_WhenResultHasMessages()
    {
        // Arrange
        var message1 = new InvalidRequestMessage("Error 1");
        var message2 = new InvalidRequestMessage("Error 2");
        var messages = new MessageBase[] { message1, message2 };
        var result = new Result(ResultStatus.Invalid, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var badRequestResult = (BadRequestObjectResult)actionResult;
        var value = badRequestResult.Value;

        value.ShouldNotBeNull();

        // Use reflection to check the anonymous type
        var messagesProperty = value.GetType().GetProperty("messages");
        messagesProperty.ShouldNotBeNull();

        var messagesValue = messagesProperty.GetValue(value) as MessageBase[];
        messagesValue.ShouldNotBeNull();
        messagesValue.Length.ShouldBe(2);
    }

    [Fact]
    public void ToActionResult_Generic_IncludesMessages_WhenResultHasMessages()
    {
        // Arrange
        var message1 = new NotFoundMessage();
        var messages = new MessageBase[] { message1 };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var notFoundResult = (NotFoundObjectResult)actionResult;
        var value = notFoundResult.Value;

        value.ShouldNotBeNull();

        // Use reflection to check the anonymous type
        var messagesProperty = value.GetType().GetProperty("messages");
        messagesProperty.ShouldNotBeNull();

        var messagesValue = messagesProperty.GetValue(value) as MessageBase[];
        messagesValue.ShouldNotBeNull();
        messagesValue.Length.ShouldBe(1);
    }

    #endregion

    // Test helper class
    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}