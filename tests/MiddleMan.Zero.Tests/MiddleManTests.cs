namespace MiddleMan.Zero.Tests;

using MiddleMan.Zero.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

public class MiddleManTests
{
    [Fact]
    public async Task MiddleMan_HandlesRequest_Successfully()
    {
        // Arrange
        var request = new DummyRequest();
        var middleMan = new DummyHandler();

        // Act
        await middleMan.HandleAsync(request);
    }

    [Fact]
    public async Task MiddleMan_WithResponse_HandlesRequest_Successfully()
    {
        // Arrange
        var request = new DummyRequest();
        var expectedResponse = new DummyResponse();

        var requestHandler = new DummyHandlerWithResponse();

        // Act
        var response = await requestHandler.HandleAsync(request);

        // Assert
        Assert.Equal(expectedResponse.GetType(), response.GetType());
    }

    [Fact]
    public async Task MiddleMan_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var requestHandler = new DummyHandler();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => requestHandler.HandleAsync(null!).AsTask());
    }

    [Fact]
    public async Task MiddleMan_WithResponse_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var requestHandler = new DummyHandlerWithResponse();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => requestHandler.HandleAsync(null!).AsTask());
    }

    public class DummyRequest { }
    public class DummyResponse { }

    public class DummyHandler : HandlerBase<DummyRequest>
    {
        protected override ValueTask HandleAsync()
        {
            // Simulate handling logic
            return new ValueTask();
        }
    }

    public class DummyHandlerWithResponse : HandlerBase<DummyRequest, DummyResponse>
    {
        protected override ValueTask<DummyResponse> HandleAsync()
        {
            // Simulate handling logic and return a response
            return new ValueTask<DummyResponse>(new DummyResponse());
        }
    }
}
