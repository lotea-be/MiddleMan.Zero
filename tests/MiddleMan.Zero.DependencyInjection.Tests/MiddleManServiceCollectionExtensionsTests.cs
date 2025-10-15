using Microsoft.Extensions.DependencyInjection;
using MiddleMan.Zero.Abstractions;

namespace MiddleMan.Zero.DependencyInjection.Tests;

public class MiddleManServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMiddleMan_RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMiddleManZero();
        var provider = services.BuildServiceProvider();

        // Assert
        var handler = provider.GetService<IHandleAsync<TestRequest>>();

        handler.ShouldSatisfyAllConditions(
            () => handler.ShouldNotBeNull(),
            () => handler.ShouldBeOfType<TestHandler>()
        );
    }

    [Fact]
    public void AddMiddleMan_RegistersHandlersWithResponse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMiddleManZero();
        var provider = services.BuildServiceProvider();

        // Assert
        var handlerWithResponse = provider.GetService<IHandleAsync<TestRequestWithResponse, string>>();

        handlerWithResponse.ShouldSatisfyAllConditions(
            () => handlerWithResponse.ShouldNotBeNull(),
            () => handlerWithResponse.ShouldBeOfType<TestHandlerWithResponse>()
        );
    }

    // Test classes
    public class TestRequest { }

    public class TestRequestWithResponse { }

    public class TestHandler : HandlerBase<TestRequest>
    {
        protected override ValueTask HandleAsync(TestRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        protected override ValueTask ValidateAsync(TestRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    public class TestHandlerWithResponse : HandlerBase<TestRequestWithResponse, string>
    {
        protected override ValueTask<string> HandleAsync(TestRequestWithResponse request, HandlerContext context, CancellationToken cancellationToken = default)
            => new("Test");

        protected override ValueTask ValidateAsync(TestRequestWithResponse request, HandlerContext context, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}