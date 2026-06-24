using MiddleMan.Zero.Abstractions;

namespace MiddleMan.Zero.Tests;

/// <summary>
/// Covers the message-class constructor overloads and shared <see cref="MessageBase"/> behavior.
/// </summary>
public class MessageTests
{
    [Fact]
    public void MessageBase_ToString_ReturnsMessageText()
    {
        var msg = new DebugMessage("hello");

        msg.ToString().ShouldBe("hello");
    }

    [Fact]
    public void MessageBase_DefaultProperties_AreInitialized()
    {
        var before = DateTime.UtcNow;

        var msg = new DebugMessage();

        msg.ShouldSatisfyAllConditions(
            () => msg.Id.ShouldNotBe(Guid.Empty),
            () => msg.CorrelationId.ShouldNotBe(Guid.Empty),
            () => msg.CreatedAt.ShouldBeGreaterThanOrEqualTo(before),
            () => msg.Message.ShouldBe(string.Empty),
            () => msg.Code.ShouldBe(string.Empty)
        );
    }

    [Theory]
    [ClassData(typeof(MessageTypeData))]
    public void Message_StringCtor_SetsMessage(Type messageType)
    {
        var msg = (MessageBase)Activator.CreateInstance(messageType, "boom")!;

        msg.ShouldSatisfyAllConditions(
            () => msg.Message.ShouldBe("boom"),
            () => msg.Code.ShouldBe(string.Empty)
        );
    }

    [Theory]
    [ClassData(typeof(MessageTypeData))]
    public void Message_StringStringCtor_SetsMessageAndCode(Type messageType)
    {
        var msg = (MessageBase)Activator.CreateInstance(messageType, "boom", "code_42")!;

        msg.ShouldSatisfyAllConditions(
            () => msg.Message.ShouldBe("boom"),
            () => msg.Code.ShouldBe("code_42")
        );
    }

    [Theory]
    [ClassData(typeof(MessageTypeData))]
    public void Message_ParameterlessCtor_Works(Type messageType)
    {
        var msg = (MessageBase)Activator.CreateInstance(messageType)!;

        msg.ShouldSatisfyAllConditions(
            () => msg.Message.ShouldBe(string.Empty),
            () => msg.Code.ShouldBe(string.Empty)
        );
    }

    private sealed class MessageTypeData : TheoryData<Type>
    {
        public MessageTypeData()
        {
            Add(typeof(DebugMessage));
            Add(typeof(FailureMessage));
            Add(typeof(ForbiddenMessage));
            Add(typeof(ConflictMessage));
            Add(typeof(NotFoundMessage));
            Add(typeof(InvalidRequestMessage));
        }
    }
}
