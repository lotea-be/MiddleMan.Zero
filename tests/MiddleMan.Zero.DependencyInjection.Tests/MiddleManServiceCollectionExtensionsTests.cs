using System.Reflection;

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

    [Fact]
    public void AddMiddleMan_WithExplicitAssemblies_RegistersOnlyHandlersInThoseAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var thisAssembly = typeof(MiddleManServiceCollectionExtensionsTests).Assembly;

        // Act - params Assembly[] overload
        services.AddMiddleManZero(thisAssembly);
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IHandleAsync<TestRequest>>().ShouldBeOfType<TestHandler>();
    }

    [Fact]
    public void AddMiddleMan_WithExplicitAssembliesAndLifetime_HonorsLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var thisAssembly = typeof(MiddleManServiceCollectionExtensionsTests).Assembly;

        // Act - IEnumerable<Assembly>, ServiceLifetime overload
        services.AddMiddleManZero(new[] { thisAssembly }, ServiceLifetime.Singleton);

        // Assert - the service descriptor must reflect Singleton lifetime
        var descriptor = services.Single(d => d.ServiceType == typeof(IHandleAsync<TestRequest>));
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddMiddleMan_CalledTwice_DoesNotProduceDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMiddleManZero();
        services.AddMiddleManZero();

        // Assert - exactly one interface descriptor and one concrete descriptor for TestHandler
        services.Count(d => d.ServiceType == typeof(IHandleAsync<TestRequest>)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(TestHandler)).ShouldBe(1);
    }

    [Fact]
    public void AddMiddleMan_SkipsHandler_WhenConsumerAlreadyRegisteredIt()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IHandleAsync<TestRequest>, TestHandler>();

        // Act
        services.AddMiddleManZero(typeof(MiddleManServiceCollectionExtensionsTests).Assembly);

        // Assert - the consumer's own registration is kept as the only one
        services.Single(d => d.ServiceType == typeof(IHandleAsync<TestRequest>)).Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMiddleMan_SkipsOpenGenericHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMiddleManZero(typeof(MiddleManServiceCollectionExtensionsTests).Assembly);

        // Assert
        services.ShouldNotContain(d => d.ServiceType == typeof(OpenGenericHandler<>));
        services.ShouldNotContain(d => d.ServiceType.ContainsGenericParameters);
    }

    [Fact]
    public void AddMiddleMan_RegistersConcreteHandlerType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMiddleManZero(typeof(MiddleManServiceCollectionExtensionsTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<TestHandler>().ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMiddleMan_WrapsHandlerInBehaviors_InRegistrationOrder()
    {
        // Arrange
        var calls = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        services.AddTransient<IHandlerBehavior<TestRequest>, OuterBehavior>();
        services.AddTransient<IHandlerBehavior<TestRequest>, InnerBehavior>();
        services.AddMiddleManZero(typeof(MiddleManServiceCollectionExtensionsTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Act
        var handler = provider.GetRequiredService<IHandleAsync<TestRequest>>();
        var result = await handler.HandleAsync(new TestRequest(), TestContext.Current.CancellationToken);

        // Assert
        handler.ShouldNotBeOfType<TestHandler>();
        result.ResultStatus.ShouldBe(ResultStatus.Successful);
        calls.ShouldBe(["outer:before", "inner:before", "inner:after", "outer:after"]);
    }

    [Fact]
    public async Task AddMiddleMan_AppliesOpenGenericBehaviors_ToHandlersWithResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient(typeof(IHandlerBehavior<,>), typeof(ShortCircuitBehavior<,>));
        services.AddMiddleManZero(typeof(MiddleManServiceCollectionExtensionsTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Act
        var handler = provider.GetRequiredService<IHandleAsync<TestRequestWithResponse, string>>();
        var result = await handler.HandleAsync(new TestRequestWithResponse(), TestContext.Current.CancellationToken);

        // Assert - the behavior short-circuited, so the handler never produced "Test"
        result.ResultStatus.ShouldBe(ResultStatus.Forbidden);
        result.Response.ShouldBeNull();
    }

    [Fact]
    public async Task AddMiddleMan_BehaviorCanReplaceCancellationToken()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IHandlerBehavior<TestRequestWithResponse, string>, CancellingBehavior>();
        services.AddMiddleManZero(typeof(MiddleManServiceCollectionExtensionsTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Act
        var handler = provider.GetRequiredService<IHandleAsync<TestRequestWithResponse, string>>();

        // Assert - the cancelled token handed to next() reaches HandlerBase
        await Should.ThrowAsync<OperationCanceledException>(
            () => handler.HandleAsync(new TestRequestWithResponse(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void AddMiddleMan_NullServices_Throws()
    {
        IServiceCollection services = null!;
        Should.Throw<ArgumentNullException>(() =>
            services.AddMiddleManZero(new[] { typeof(MiddleManServiceCollectionExtensionsTests).Assembly }, ServiceLifetime.Transient));
    }

    [Fact]
    public void AddMiddleMan_NullAssemblies_Throws()
    {
        var services = new ServiceCollection();
        Should.Throw<ArgumentNullException>(() =>
            services.AddMiddleManZero((IEnumerable<Assembly>)null!, ServiceLifetime.Transient));
    }

    [Fact]
    public void AddMiddleMan_AssemblyWithLoadFailures_SkipsUnloadableTypesAndContinues()
    {
        // Arrange
        var services = new ServiceCollection();
        var brokenAssembly = new ThrowingAssembly();
        var thisAssembly = typeof(MiddleManServiceCollectionExtensionsTests).Assembly;

        // Act - the broken assembly raises ReflectionTypeLoadException from GetTypes(); we still
        // expect handlers from the good assembly to be registered.
        services.AddMiddleManZero(new Assembly[] { brokenAssembly, thisAssembly }, ServiceLifetime.Transient);
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IHandleAsync<TestRequest>>().ShouldBeOfType<TestHandler>();
    }

    /// <summary>
    /// Test double whose <c>GetTypes</c> raises <see cref="ReflectionTypeLoadException"/> just like
    /// a real assembly with unresolvable type references. We surface a non-null and a null entry to
    /// exercise the loadable-type filter.
    /// </summary>
    private sealed class ThrowingAssembly : Assembly
    {
        public override Type[] GetTypes() =>
            throw new ReflectionTypeLoadException(
                new Type?[] { typeof(TestHandler), null },
                new Exception?[] { new TypeLoadException("simulated") });
    }

    // Behaviors
    public sealed class OuterBehavior(List<string> calls) : IHandlerBehavior<TestRequest>
    {
        public async Task<ResultBase> HandleAsync(TestRequest request, HandlerDelegate next, CancellationToken cancellationToken)
        {
            calls.Add("outer:before");
            var result = await next(cancellationToken);
            calls.Add("outer:after");
            return result;
        }
    }

    public sealed class InnerBehavior(List<string> calls) : IHandlerBehavior<TestRequest>
    {
        public async Task<ResultBase> HandleAsync(TestRequest request, HandlerDelegate next, CancellationToken cancellationToken)
        {
            calls.Add("inner:before");
            var result = await next(cancellationToken);
            calls.Add("inner:after");
            return result;
        }
    }

    public sealed class ShortCircuitBehavior<TRequest, TResponse> : IHandlerBehavior<TRequest, TResponse>
    {
        public Task<ResultBase<TResponse>> HandleAsync(TRequest request, HandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => Task.FromResult<ResultBase<TResponse>>(new Result<TResponse>(default, ResultStatus.Forbidden, [new ForbiddenMessage()]));
    }

    public sealed class CancellingBehavior : IHandlerBehavior<TestRequestWithResponse, string>
    {
        public Task<ResultBase<string>> HandleAsync(TestRequestWithResponse request, HandlerDelegate<string> next, CancellationToken cancellationToken)
            => next(new CancellationToken(canceled: true));
    }

    // Test classes
    public class TestRequest { }

    public class TestRequestWithResponse { }

    public class TestHandler : HandlerBase<TestRequest>
    {
        protected override Task HandleAsync(TestRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        protected override Task ValidateAsync(TestRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    public class OpenGenericHandler<TRequest> : HandlerBase<TRequest>
    {
        protected override Task HandleAsync(TRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        protected override Task ValidateAsync(TRequest request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    public class TestHandlerWithResponse : HandlerBase<TestRequestWithResponse, string>
    {
        protected override Task<string?> HandleAsync(TestRequestWithResponse request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("Test");

        protected override Task ValidateAsync(TestRequestWithResponse request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}