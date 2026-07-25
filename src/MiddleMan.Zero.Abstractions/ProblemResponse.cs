using System.Text.Json.Serialization;

namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Represents an RFC 9457 / RFC 7807 HTTP problem detail body returned for all non-success
/// handler results. Use <see cref="FromResult"/> to construct an instance from a
/// <see cref="ResultBase"/>.
/// </summary>
public sealed record ProblemResponse
{
    /// <summary>
    /// Gets a URI reference that identifies the problem type.
    /// Points to a human-readable document describing the error class.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Gets a short, human-readable summary of the problem type.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>
    /// Gets the HTTP status code applicable to this problem.
    /// </summary>
    [JsonPropertyName("status")]
    public required int Status { get; init; }

    /// <summary>
    /// Gets a human-readable explanation specific to this occurrence of the problem.
    /// When the handler logged messages, this is the joined message text; otherwise a
    /// per-status default string is used.
    /// </summary>
    [JsonPropertyName("detail")]
    public required string Detail { get; init; }

    /// <summary>
    /// Gets the optional trace identifier for correlating this problem with telemetry.
    /// When <see langword="null"/> (the current default) the field is omitted from the
    /// serialized JSON body.
    /// </summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the list of individual error messages logged by the handler.
    /// Projected from <see cref="ResultBase.Messages"/> — each entry exposes only
    /// <see cref="ErrorMessage.Message"/> and <see cref="ErrorMessage.Code"/>.
    /// Never <see langword="null"/>; defaults to an empty list.
    /// </summary>
    [JsonPropertyName("messages")]
    public IReadOnlyList<ErrorMessage> Messages { get; init; } = [];

    private const string BaseTypeUri =
        "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/";

    /// <summary>
    /// Creates a <see cref="ProblemResponse"/> from a non-success <see cref="ResultBase"/>.
    /// </summary>
    /// <param name="result">The handler result to map. Must not have status <see cref="ResultStatus.Successful"/>.</param>
    /// <param name="traceId">Optional trace identifier to embed in the response. Defaults to <see langword="null"/>.</param>
    /// <returns>A fully populated <see cref="ProblemResponse"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="result"/> has status <see cref="ResultStatus.Successful"/>,
    /// because a success result has no error body.
    /// </exception>
    public static ProblemResponse FromResult(ResultBase result, string? traceId = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var projectedMessages = result.Messages
            .Select(m => new ErrorMessage { Message = m.Message, Code = m.Code })
            .ToList();

        var joinedMessages = string.Join("; ", result.Messages
            .Select(m => m.Message)
            .Where(m => !string.IsNullOrEmpty(m)));

        return result.ResultStatus switch
        {
            ResultStatus.Invalid => Build(
                status: 400,
                title: "Bad Request",
                slug: "bad-request",
                defaultDetail: "The request is invalid.",
                joinedMessages: joinedMessages,
                projectedMessages: projectedMessages,
                traceId: traceId),

            ResultStatus.Forbidden => Build(
                status: 403,
                title: "Forbidden",
                slug: "forbidden",
                defaultDetail: "Access denied.",
                joinedMessages: joinedMessages,
                projectedMessages: projectedMessages,
                traceId: traceId),

            ResultStatus.NotFound => Build(
                status: 404,
                title: "Not Found",
                slug: "not-found",
                defaultDetail: "The requested resource was not found.",
                joinedMessages: joinedMessages,
                projectedMessages: projectedMessages,
                traceId: traceId),

            ResultStatus.Conflict => Build(
                status: 409,
                title: "Conflict",
                slug: "conflict",
                defaultDetail: "The request conflicts with the current state.",
                joinedMessages: joinedMessages,
                projectedMessages: projectedMessages,
                traceId: traceId),

            ResultStatus.Successful => throw new InvalidOperationException(
                "Cannot create a ProblemResponse from a successful result."),

            // Failure, Undefined, and any future unmapped value all map to 500.
            _ => Build(
                status: 500,
                title: "Internal Server Error",
                slug: "internal-server-error",
                defaultDetail: "An unexpected error occurred.",
                joinedMessages: joinedMessages,
                projectedMessages: projectedMessages,
                traceId: traceId),
        };
    }

    private static ProblemResponse Build(
        int status,
        string title,
        string slug,
        string defaultDetail,
        string joinedMessages,
        IReadOnlyList<ErrorMessage> projectedMessages,
        string? traceId) =>
        new()
        {
            Status = status,
            Title = title,
            Type = $"{BaseTypeUri}{slug}.md",
            Detail = string.IsNullOrEmpty(joinedMessages) ? defaultDetail : joinedMessages,
            Messages = projectedMessages,
            TraceId = traceId,
        };
}