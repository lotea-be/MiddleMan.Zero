namespace MiddleMan.Zero.AspNetCore.Mvc.Tests;

public class ResultFilterTests
{
    private readonly ResultFilter _filter;

    public ResultFilterTests()
    {
        _filter = new ResultFilter();
    }

    #region Non-Generic Result Tests

    [Fact]
    public void OnResultExecuting_ConvertsNonGenericSuccessfulResult_ToOkResult()
    {
        // Arrange
        var result = new Result(ResultStatus.Successful, []);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public void OnResultExecuting_ConvertsNonGenericNotFoundResult_ToNotFoundObjectResult()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result(ResultStatus.NotFound, messages);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)context.Result;
        notFoundResult.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void OnResultExecuting_ConvertsNonGenericInvalidResult_ToBadRequestObjectResult()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };
        var result = new Result(ResultStatus.Invalid, messages);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)context.Result;
        badRequestResult.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void OnResultExecuting_ConvertsNonGenericFailureResult_ToObjectResultWith500()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result(ResultStatus.Failure, messages);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)context.Result;
        objectResult.StatusCode.ShouldBe(500);
    }

    #endregion

    #region Generic Result<TResponse> Tests

    [Fact]
    public void OnResultExecuting_ConvertsGenericSuccessfulResult_ToOkObjectResult()
    {
        // Arrange
        var response = new TestResponse { Id = 1, Name = "Test" };
        var result = new Result<TestResponse>(response, ResultStatus.Successful, []);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)context.Result;
        okResult.Value.ShouldBe(response);
    }

    [Fact]
    public void OnResultExecuting_ConvertsGenericNotFoundResult_ToNotFoundObjectResult()
    {
        // Arrange
        var messages = new MessageBase[] { new NotFoundMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.NotFound, messages);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)context.Result;
        notFoundResult.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void OnResultExecuting_ConvertsGenericInvalidResult_ToBadRequestObjectResult()
    {
        // Arrange
        var messages = new MessageBase[] { new InvalidRequestMessage("Invalid") };
        var result = new Result<TestResponse>(null, ResultStatus.Invalid, messages);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)context.Result;
        badRequestResult.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void OnResultExecuting_ConvertsGenericFailureResult_ToObjectResultWith500()
    {
        // Arrange
        var messages = new MessageBase[] { new FailureMessage() };
        var result = new Result<TestResponse>(null, ResultStatus.Failure, messages);
        var context = CreateResultExecutingContext(new ObjectResult(result));

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)context.Result;
        objectResult.StatusCode.ShouldBe(500);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void OnResultExecuting_DoesNotConvert_WhenResultIsNotObjectResult()
    {
        // Arrange
        var originalResult = new OkResult();
        var context = CreateResultExecutingContext(originalResult);

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBe(originalResult);
    }

    [Fact]
    public void OnResultExecuting_DoesNotConvert_WhenObjectResultValueIsNotResultBase()
    {
        // Arrange
        var originalResult = new ObjectResult("Some string value");
        var context = CreateResultExecutingContext(originalResult);

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        context.Result.ShouldBe(originalResult);
    }

    [Fact]
    public void OnResultExecuted_DoesNothing()
    {
        // Arrange
        var context = CreateResultExecutedContext();

        // Act & Assert (should not throw)
        _filter.OnResultExecuted(context);
    }

    #endregion

    #region Helper Methods

    private static ResultExecutingContext CreateResultExecutingContext(IActionResult result)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        return new ResultExecutingContext(
            actionContext,
            [],
            result,
            controller: null!
        );
    }

    private static ResultExecutedContext CreateResultExecutedContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        return new ResultExecutedContext(
            actionContext,
            [],
            new OkResult(),
            controller: null!
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