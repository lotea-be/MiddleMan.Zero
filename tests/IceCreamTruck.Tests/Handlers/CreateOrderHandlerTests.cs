using IceCreamTruck.Contracts;
using IceCreamTruck.Handlers;

using MiddleMan.Zero;
using MiddleMan.Zero.Abstractions;

namespace IceCreamTruck.Tests.Handlers;

/// <summary>
/// Tests demonstrating MiddleMan.Zero features with CreateOrderHandler.
/// These tests focus on:
/// - Handler without response type (void handlers)
/// - Multiple validation rules
/// - HandlerContext for validation state
/// </summary>
public class CreateOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WithValidRequest()
    {
        // Arrange
        var handler = new CreateOrderHandler();
        var request = new CreateOrderRequest
        {
            CustomerName = "Jane Smith",
            Items = [new IceCream { Flavor = "Chocolate", Price = 6.50m, Scoops = 3 }]
        };

        // Act - MiddleMan.Zero orchestrates validation and handling
        var result = await handler.HandleAsync(request);

        // Assert - Demonstrates successful Result for handler without response type
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful)
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenCustomerNameIsEmpty()
    {
        // Arrange
        var handler = new CreateOrderHandler();
        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = [new IceCream { Flavor = "Vanilla", Price = 5.00m, Scoops = 1 }]
        };

        // Act - Validation logs InvalidRequestMessage
        var result = await handler.HandleAsync(request);

        // Assert - MiddleMan.Zero converts to Invalid status
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => invalidMessage.Message.ShouldBe("Customer name is required.")
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenItemsAreEmpty()
    {
        // Arrange
        var handler = new CreateOrderHandler();
        var request = new CreateOrderRequest
        {
            CustomerName = "John",
            Items = []
        };

        // Act
        var result = await handler.HandleAsync(request);

        // Assert - Demonstrates validation message content
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => invalidMessage.Message.ShouldBe("Order must contain at least one ice cream.")
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WithMultipleValidationErrors()
    {
        // Arrange
        var handler = new CreateOrderHandler();
        var request = new CreateOrderRequest
        {
            CustomerName = "   ",  // Whitespace only
            Items = []             // Empty list
        };

        // Act - Multiple validation rules can fail
        var result = await handler.HandleAsync(request);

        // Assert - Demonstrates HandlerContext can collect multiple validation messages
        var invalidMessages = result.Messages.OfType<InvalidRequestMessage>().ToList();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => invalidMessages.Count.ShouldBe(2),
            () => invalidMessages.Any(m => m.Message.Contains("Customer name")).ShouldBeTrue(),
            () => invalidMessages.Any(m => m.Message.Contains("at least one ice cream")).ShouldBeTrue()
        );
    }


}