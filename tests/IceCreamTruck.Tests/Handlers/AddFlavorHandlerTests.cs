using FakeItEasy;

using IceCreamTruck.Contracts;
using IceCreamTruck.Handlers;
using IceCreamTruck.Repositories;

using MiddleMan.Zero;
using MiddleMan.Zero.Abstractions;

namespace IceCreamTruck.Tests.Handlers;

/// <summary>
/// Tests demonstrating MiddleMan.Zero features with AddFlavorHandler.
/// These tests focus on:
/// - Handler without response type (void handlers)
/// - Multiple validation rules (empty name, duplicate flavor)
/// - HandlerContext for validation state
/// </summary>
public class AddFlavorHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WithValidRequest()
    {
        // Arrange
        var flavorRepository = A.Fake<IFlavorRepository>();
        A.CallTo(() => flavorRepository.GetAsync("Mint", A<CancellationToken>._))
            .Returns(Task.FromResult<Flavor?>(null));

        var handler = new AddFlavorHandler(flavorRepository);
        var request = new AddFlavorRequest { FlavorName = "Mint" };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful),
            () => result.Messages.ShouldBeEmpty()
        );

        A.CallTo(() => flavorRepository.AddAsync("Mint", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenFlavorNameIsEmpty()
    {
        // Arrange
        var flavorRepository = A.Fake<IFlavorRepository>();
        var handler = new AddFlavorHandler(flavorRepository);
        var request = new AddFlavorRequest { FlavorName = "" };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => invalidMessage.Message.ShouldBe("Flavor name is required.")
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenFlavorNameIsWhitespace()
    {
        // Arrange
        var flavorRepository = A.Fake<IFlavorRepository>();
        var handler = new AddFlavorHandler(flavorRepository);
        var request = new AddFlavorRequest { FlavorName = "   " };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Messages.OfType<InvalidRequestMessage>().ShouldNotBeEmpty()
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenFlavorAlreadyExists()
    {
        // Arrange
        var flavorRepository = A.Fake<IFlavorRepository>();
        A.CallTo(() => flavorRepository.GetAsync("Vanilla", A<CancellationToken>._))
            .Returns(Task.FromResult<Flavor?>(new Flavor { Name = "Vanilla" }));

        var handler = new AddFlavorHandler(flavorRepository);
        var request = new AddFlavorRequest { FlavorName = "Vanilla" };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => invalidMessage.Message.ShouldBe("Flavor 'Vanilla' already exists.")
        );
    }

    [Fact]
    public async Task HandleAsync_DoesNotCallRepository_WhenValidationFails()
    {
        // Arrange
        var flavorRepository = A.Fake<IFlavorRepository>();
        var handler = new AddFlavorHandler(flavorRepository);
        var request = new AddFlavorRequest { FlavorName = "" };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert - Demonstrates fail-fast pattern: validation fails, handler never executes
        result.ResultStatus.ShouldBe(ResultStatus.Invalid);

        A.CallTo(() => flavorRepository.AddAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
