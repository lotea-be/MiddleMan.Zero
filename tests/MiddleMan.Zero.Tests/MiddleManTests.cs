using MiddleMan.Zero.Abstractions;
using Xunit;

namespace MiddleMan.Zero.Tests;
public class MiddleManTests
{
    [Fact]
    public async Task MiddleMan_HandlesRequest_Successfully()
    {
        // Arrange
        var request = new DummyRequest() { MyInput = "Foo"};
        var middleMan = new DummyHandler();

        // Act
        await middleMan.HandleAsync(request);
    }

    [Fact]
    public async Task MiddleMan_WithResponse_HandlesRequest_Successfully()
    {
        // Arrange
        var request = new DummyRequest() { MyInput = "Foo"};
        var expectedResult = new Result<DummyResponse>(
            new() { MyOutput = "Hello Foo!" },
            Abstractions.ResultStatus.Succesful, []);

        var requestHandler = new DummyHandlerWithResponse();

        // Act
        var result = await requestHandler.HandleAsync(request);

        // Assert
        Assert.Equal(expectedResult.GetType(), result.GetType());
        Assert.Equal(expectedResult.ResultStatus, result.ResultStatus);
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
        Assert.Equal(ResultStatus.Invalid, result.ResultStatus);
        Assert.Single(result.Messages);

        var message = result.Messages[0];

        Assert.Equal(expectedLogMessage.GetType(), message.GetType());
        Assert.Equal(expectedLogMessage.Code, message.Code);
        Assert.Equal(expectedLogMessage.Message, message.Message);
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
        Assert.Equal(ResultStatus.Invalid, result.ResultStatus);
        Assert.Single(result.Messages);

        var message = result.Messages[0];

        Assert.Equal(expectedLogMessage.GetType(), message.GetType());
        Assert.Equal(expectedLogMessage.Code, message.Code);
        Assert.Equal(expectedLogMessage.Message, message.Message);
    }

    public class DummyRequest { public required string MyInput { get; set; } }
    public class DummyResponse { public required string MyOutput { get; set; } }

    public class DummyHandler : HandlerBase<DummyRequest>
    {
        protected override ValueTask HandleAsync(DummyRequest request, HandlerContext context)
        {
            // Simulate handling logic
            return ValueTask.CompletedTask;
        }

        protected override ValueTask ValidateAsync(DummyRequest request, HandlerContext context)
        {
            return ValueTask.CompletedTask;
        }
    }

    public class DummyHandlerWithResponse : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override ValueTask ValidateAsync(DummyRequest request, HandlerContext context)
        {
            if (string.IsNullOrEmpty(request.MyInput))
            {
                context.LogMessage(new InvalidRequestMessage("MyInput is null or empty.", "dummy_myinput_null"));
            }

            return ValueTask.CompletedTask;
        }

        protected override ValueTask<DummyResponse> HandleAsync(DummyRequest request, HandlerContext context)
        {
            // Simulate handling logic and return a response
            return new ValueTask<DummyResponse>(new DummyResponse() { MyOutput = $"Hello {request.MyInput}!"});
        }
    }
}
