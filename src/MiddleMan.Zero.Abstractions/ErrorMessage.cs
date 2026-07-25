using System.Text.Json.Serialization;

namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// A projection of a handler message for inclusion in an HTTP problem response body.
/// Only exposes the human-readable message text and the machine-readable code; the internal
/// tracking fields (<c>Id</c>, <c>CorrelationId</c>, <c>CreatedAt</c>) are intentionally omitted.
/// </summary>
public sealed record ErrorMessage
{
    /// <summary>
    /// Gets the human-readable description of the error.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>
    /// Gets the machine-readable code that categorizes or identifies the error.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }
}