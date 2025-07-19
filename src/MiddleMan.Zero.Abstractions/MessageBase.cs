namespace MiddleMan.Zero.Abstractions;

public abstract class MessageBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}