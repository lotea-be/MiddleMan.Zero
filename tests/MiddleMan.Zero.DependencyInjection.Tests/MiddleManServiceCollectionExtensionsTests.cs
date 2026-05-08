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

        // Assert - exactly one (TestRequest -> TestHandler) descriptor
        var matching = services.Where(d =>
            d.ServiceType == typeof(IHandleAsync<TestRequest>) &&
            d.ImplementationType == typeof(TestHandler));
        matching.Count().ShouldBe(1);
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

    public class TestHandlerWithResponse : HandlerBase<TestRequestWithResponse, string>
    {
        protected override Task<string?> HandleAsync(TestRequestWithResponse request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("Test");

        protected override Task ValidateAsync(TestRequestWithResponse request, HandlerContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}