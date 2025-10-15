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
        await middleMan.HandleAsync(request);
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
        var result = await requestHandler.HandleAsync(request);

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
        var result = await requestHandler.HandleAsync(null!);

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
        var result = await requestHandler.HandleAsync(null!);

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

    public class DummyRequest { public required string MyInput { get; set; } }
    public class DummyResponse { public required string MyOutput { get; set; } }

    public class DummyHandler : HandlerBase<DummyRequest>
    {
        protected override ValueTask HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Simulate handling logic
            return ValueTask.CompletedTask;
        }

        protected override ValueTask ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }

    public class DummyHandlerWithResponse : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override ValueTask ValidateAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.MyInput))
            {
                context.Log(new InvalidRequestMessage("MyInput is null or empty.", "dummy_myinput_null"));
            }

            return ValueTask.CompletedTask;
        }

        protected override ValueTask<DummyResponse> HandleAsync(DummyRequest request, HandlerContext context, CancellationToken cancellationToken = default)
        {
            // Simulate handling logic and return a response
            return new ValueTask<DummyResponse>(new DummyResponse() { MyOutput = $"Hello {request.MyInput}!" });
        }
    }
}
