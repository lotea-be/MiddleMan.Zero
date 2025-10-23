using Microsoft.Extensions.DependencyInjection;

namespace MiddleMan.Zero.AspNetCore.Mvc.Tests;

public class MvcBuilderExtensionsTests
{
    [Fact]
    public void AddMiddleManZeroResults_IMvcBuilder_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddMvcCore();

        // Act
        var result = mvcBuilder.AddMiddleManZeroResults();

        // Assert
        result.ShouldBe(mvcBuilder);
    }

    [Fact]
    public void AddMiddleManZeroResults_IMvcCoreBuilder_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcCoreBuilder = services.AddMvcCore();

        // Act
        var result = mvcCoreBuilder.AddMiddleManZeroResults();

        // Assert
        result.ShouldBe(mvcCoreBuilder);
    }

    [Fact]
    public void AddMiddleManZeroResults_IMvcBuilder_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddMvcCore();

        // Act & Assert - should not throw
        Should.NotThrow(() => mvcBuilder.AddMiddleManZeroResults());
    }

    [Fact]
    public void AddMiddleManZeroResults_IMvcCoreBuilder_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcCoreBuilder = services.AddMvcCore();

        // Act & Assert - should not throw
        Should.NotThrow(() => mvcCoreBuilder.AddMiddleManZeroResults());
    }
}