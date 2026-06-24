using MiddleMan.Zero.Abstractions;

namespace MiddleMan.Zero.Tests;

public class MiddleManTests
{
    [Fact]
    public async Task MiddleMan_HandlesRequest_Successfully()
    {
        // Arrange
        var request = new DummyRequest() { MyInput = "Foo" };
        var middleMan = new DummyHandler();

        // Act
        await middleMan.HandleAsync(request, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MiddleMan_WithResponse_HandlesRequest_Successfully()
    {
        // Arrange
        var request = new DummyRequest() { MyInput = "Foo" };
        var expectedResult = new Result<DummyResponse>(
            new() { MyOutput = "Hello Foo!" },
            Abstractions.ResultStatus.Successful, []);

        var requestHandler = new DummyHandlerWithResponse();

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.GetType().ShouldBe(expectedResult.GetType()),
            () => result.ResultStatus.ShouldBe(expectedResult.ResultStatus)
        );
    }

    [Fact]
    public async Task MiddleMan_LogsInvalidRequest_WhenRequestIsNull()
    {
        // Arrange
        var requestHandler = new DummyHandler();
        var expectedLogMessage = new InvalidRequestMessage("Request is null.", "middleman_request_null");

        // Act
        var result = await requestHandler.HandleAsync(null!, TestContext.Current.CancellationToken);

        // Assert
        var message = result.Messages[0];

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Messages.ShouldHaveSingleItem(),
            () => message.GetType().ShouldBe(expectedLogMessage.GetType()),
            () => message.Code.ShouldBe(expectedLogMessage.Code),
            () => message.Message.ShouldBe(expectedLogMessage.Message)
        );
    }

    [Fact]
    public async Task MiddleMan_WithResponse_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var requestHandler = new DummyHandlerWithResponse();
        var expectedLogMessage = new InvalidRequestMessage("Request is null.", "middleman_request_null");

        // Act
        var result = await requestHandler.HandleAsync(null!, TestContext.Current.CancellationToken  );

        // Assert
        var message = result.Messages[0];

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Messages.ShouldHaveSingleItem(),
            () => message.GetType().ShouldBe(expectedLogMessage.GetType()),
            () => message.Code.ShouldBe(expectedLogMessage.Code),
            () => message.Message.ShouldBe(expectedLogMessage.Message)
        );
    }

    [Fact]
    public async Task MiddleMan_ReturnsFailure_WhenHandlerLogsFailureMessage()
    {
        // Arrange
        var request = new DummyRequest() { MyInput = "Foo" };
        var requestHandler = new DummyHandlerWithFailure();
        var expectedMessage = "An error occurred during processing.";

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var message = result.Messages[0];

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Failure),
            () => result.Messages.ShouldHaveSingleItem(),
            () => message.ShouldBeOfType<FailureMessage>(),
            () => message.Message.ShouldBe(expectedMessage)
        );
    }

    [Fact]
    public async Task MiddleMan_WithResponse_ReturnsFailure_WhenHandlerLogsFailureMessage()
    {
        // Arrange
        var request = new DummyRequest() { MyInput = "Foo" };
        var requestHandler = new DummyHandlerWithResponseFailure();
        var expectedMessage = "Failed to retrieve response.";

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken    );

        // Assert
        var message = result.Messages[0];

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Failure),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.ShouldHaveSingleItem(),
            () => message.ShouldBeOfType<FailureMessage>(),
            () => message.Message.ShouldBe(expectedMessage)
        );
    }

    [Fact]
    public async Task MiddleMan_WithResponse_ReturnsNotFound_WhenResourceNotFound()
    {
        // Arrange
        var request = new DummyRequest() { MyInput = "NonExistent" };
        var requestHandler = new DummyHandlerWithNotFound();

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var message = result.Messages[0];

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.NotFound),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.ShouldHaveSingleItem(),
            () => message.ShouldBeOfType<NotFoundMessage>()
        );
    }

    [Fact]
    public async Task MiddleMan_ReturnsForbidden_WhenHandlerLogsForbiddenMessage()
    {
        // Arrange - non-generic (void) handler path
        var request = new DummyRequest() { MyInput = "Foo" };
        var requestHandler = new DummyHandlerWithForbidden();

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Forbidden),
            () => result.Messages.ShouldHaveSingleItem(),
            () => result.Messages[0].ShouldBeOfType<ForbiddenMessage>()
        );
    }

    [Fact]
    public async Task MiddleMan_WithResponse_ReturnsForbidden_WhenHandlerLogsForbiddenMessage()
    {
        // Arrange - generic handler path (exercises the fixed CreateResult overload)
        var request = new DummyRequest() { MyInput = "Foo" };
        var requestHandler = new DummyHandlerWithResponseForbidden();

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Forbidden),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.ShouldHaveSingleItem(),
            () => result.Messages[0].ShouldBeOfType<ForbiddenMessage>()
        );
    }

    [Fact]
    public async Task MiddleMan_ReturnsConflict_WhenHandlerLogsConflictMessage()
    {
        // Arrange - non-generic (void) handler path
        var request = new DummyRequest() { MyInput = "Foo" };
        var requestHandler = new DummyHandlerWithConflict();

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Conflict),
            () => result.Messages.ShouldHaveSingleItem(),
            () => result.Messages[0].ShouldBeOfType<ConflictMessage>()
        );
    }

    [Fact]
    public async Task MiddleMan_WithResponse_ReturnsConflict_WhenHandlerLogsConflictMessage()
    {
        // Arrange - generic handler path
        var request = new DummyRequest() { MyInput = "Foo" };
        var requestHandler = new DummyHandlerWithResponseConflict();

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Conflict),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.ShouldHaveSingleItem(),
            () => result.Messages[0].ShouldBeOfType<ConflictMessage>()
        );
    }

    [Fact]
    public async Task MiddleMan_InvalidWinsOverConflict_WhenBothAreLogged()
    {
        // Arrange - validation fails before HandleAsync runs, so Conflict can't co-exist via the
        // normal pipeline; assert precedence directly through a handler that logs both during validation.
        var request = new DummyRequest() { MyInput = "Foo" };
        var requestHandler = new DummyHandlerWithInvalidAndConflict();

        // Act
        var result = await requestHandler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert - Invalid precedence: HandleAsync never runs, but if both messages co-existed,
        // CreateResult must still surface Invalid over Conflict.
        result.ResultStatus.ShouldBe(ResultStatus.Invalid);
    }

    public class DummyRequest { public required string MyInput { get; set; } }
    public class DummyResponse { public required string MyOutput { get; set; } }

    public class DummyHandler : HandlerBase<DummyRequest>
    {
        protected override Task HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Simulate handling logic
            return Task.CompletedTask;
        }

        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class DummyHandlerWithResponse : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.MyInput))
            {
                context.Log(new InvalidRequestMessage("MyInput is null or empty.", "dummy_myinput_null"));
            }

            return Task.CompletedTask;
        }

        protected override Task<DummyResponse?> HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Simulate handling logic and return a response
            return Task.FromResult<DummyResponse?>(new DummyResponse() { MyOutput = $"Hello {request.MyInput}!" });
        }
    }

    public class DummyHandlerWithFailure : HandlerBase<DummyRequest>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected override Task HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Simulate a failure during handling
            context.Log(new FailureMessage { Message = "An error occurred during processing." });
            return Task.CompletedTask;
        }
    }

    public class DummyHandlerWithResponseFailure : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected override Task<DummyResponse?> HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Simulate a failure during handling
            context.Log(new FailureMessage { Message = "Failed to retrieve response." });
            return Task.FromResult<DummyResponse?>(null);
        }
    }

    public class DummyHandlerWithNotFound : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected override Task<DummyResponse?> HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Simulate resource not found
            context.Log(new NotFoundMessage { Message = "Resource not found." });
            return Task.FromResult<DummyResponse?>(null);
        }
    }

    public class DummyHandlerWithForbidden : HandlerBase<DummyRequest>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        protected override Task HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            context.Log(new ForbiddenMessage());
            return Task.CompletedTask;
        }
    }

    public class DummyHandlerWithResponseForbidden : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        protected override Task<DummyResponse?> HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            context.Log(new ForbiddenMessage());
            return Task.FromResult<DummyResponse?>(null);
        }
    }

    public class DummyHandlerWithConflict : HandlerBase<DummyRequest>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        protected override Task HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            context.Log(new ConflictMessage("Resource state conflicts with request.", "dummy_conflict"));
            return Task.CompletedTask;
        }
    }

    public class DummyHandlerWithResponseConflict : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        protected override Task<DummyResponse?> HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            context.Log(new ConflictMessage("Resource state conflicts with request.", "dummy_conflict"));
            return Task.FromResult<DummyResponse?>(null);
        }
    }

    public class DummyHandlerWithInvalidAndConflict : HandlerBase<DummyRequest>
    {
        protected override Task ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Log both to exercise the Invalid-over-Conflict precedence path in CreateResult.
            context.Log(new ConflictMessage());
            context.Log(new InvalidRequestMessage("Invalid."));
            return Task.CompletedTask;
        }

        protected override Task HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}