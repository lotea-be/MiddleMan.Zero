using System.Text.Json;
using MiddleMan.Zero.Abstractions;

namespace MiddleMan.Zero.Tests;

/// <summary>
/// Unit tests for <see cref="ProblemResponse"/> and <see cref="ErrorMessage"/>.
/// Covers: record projection, JSON serialization, factory status mapping,
/// Detail population strategy, and guard clauses.
/// </summary>
public class ProblemResponseTests
{
    // -------------------------------------------------------------------------
    // ErrorMessage -- projection shape
    // -------------------------------------------------------------------------

    [Fact]
    public void ErrorMessage_OnlyExposesMessageAndCode()
    {
        var msg = new ErrorMessage { Message = "Something went wrong", Code = "err_42" };

        msg.ShouldSatisfyAllConditions(
            () => msg.Message.ShouldBe("Something went wrong"),
            () => msg.Code.ShouldBe("err_42")
        );

        // The projection must NOT have Id / CorrelationId / CreatedAt
        var type = typeof(ErrorMessage);
        type.GetProperty("Id").ShouldBeNull();
        type.GetProperty("CorrelationId").ShouldBeNull();
        type.GetProperty("CreatedAt").ShouldBeNull();
    }

    [Fact]
    public void ErrorMessage_JsonSerialization_EmitsOnlyMessageAndCode()
    {
        var msg = new ErrorMessage { Message = "Bad input", Code = "invalid_field" };
        var json = JsonSerializer.Serialize(msg);

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("message", out _).ShouldBeTrue();
        root.TryGetProperty("code", out _).ShouldBeTrue();
        root.TryGetProperty("id", out _).ShouldBeFalse();
        root.TryGetProperty("correlationId", out _).ShouldBeFalse();
        root.TryGetProperty("createdAt", out _).ShouldBeFalse();
    }

    // -------------------------------------------------------------------------
    // ProblemResponse -- JSON serialization shape
    // -------------------------------------------------------------------------

    [Fact]
    public void ProblemResponse_JsonSerialization_RequiredFieldsPresent_WhenTraceIdIsNull()
    {
        var result = new Result(ResultStatus.Failure, []);
        var problem = ProblemResponse.FromResult(result);

        var json = JsonSerializer.Serialize(problem);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Required fields always present
        root.TryGetProperty("type", out _).ShouldBeTrue();
        root.TryGetProperty("title", out _).ShouldBeTrue();
        root.TryGetProperty("status", out _).ShouldBeTrue();
        root.TryGetProperty("detail", out _).ShouldBeTrue();
        root.TryGetProperty("messages", out _).ShouldBeTrue();

        // TraceId is null -- must be absent from JSON
        root.TryGetProperty("traceId", out _).ShouldBeFalse();
    }

    [Fact]
    public void ProblemResponse_JsonSerialization_TraceIdPresent_WhenProvided()
    {
        var result = new Result(ResultStatus.Failure, []);
        var problem = ProblemResponse.FromResult(result, traceId: "trace-abc");

        var json = JsonSerializer.Serialize(problem);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("traceId", out var traceEl).ShouldBeTrue();
        traceEl.GetString().ShouldBe("trace-abc");
    }

    // -------------------------------------------------------------------------
    // FromResult -- every non-success status maps correctly
    // -------------------------------------------------------------------------

    public static TheoryData<ResultStatus, int, string, string> NonSuccessStatusMappings =>
        new()
        {
            { ResultStatus.Invalid,   400, "Bad Request",            "bad-request" },
            { ResultStatus.Forbidden, 403, "Forbidden",              "forbidden" },
            { ResultStatus.NotFound,  404, "Not Found",              "not-found" },
            { ResultStatus.Conflict,  409, "Conflict",               "conflict" },
            { ResultStatus.Failure,   500, "Internal Server Error",  "internal-server-error" },
            { ResultStatus.Undefined, 500, "Internal Server Error",  "internal-server-error" },
        };

    [Theory]
    [MemberData(nameof(NonSuccessStatusMappings))]
    public void FromResult_MapsEveryNonSuccessStatus(
        ResultStatus status,
        int expectedHttpStatus,
        string expectedTitle,
        string expectedSlug)
    {
        var result = new Result(status, []);
        var problem = ProblemResponse.FromResult(result);

        problem.ShouldSatisfyAllConditions(
            () => problem.Status.ShouldBe(expectedHttpStatus),
            () => problem.Title.ShouldBe(expectedTitle),
            () => problem.Type.ShouldEndWith($"{expectedSlug}.md")
        );
    }

    [Fact]
    public void FromResult_ThrowsInvalidOperationException_OnSuccessful()
    {
        var result = new Result(ResultStatus.Successful, []);

        Should.Throw<InvalidOperationException>(
            () => ProblemResponse.FromResult(result));
    }

    // -------------------------------------------------------------------------
    // Detail population: default string vs. joined messages
    // -------------------------------------------------------------------------

    [Fact]
    public void FromResult_UsesDefaultDetail_WhenNoMessages()
    {
        var result = new Result(ResultStatus.NotFound, []);
        var problem = ProblemResponse.FromResult(result);

        problem.Detail.ShouldBe("The requested resource was not found.");
    }

    [Fact]
    public void FromResult_UsesJoinedMessages_WhenMessagesPresent()
    {
        var messages = new MessageBase[]
        {
            new NotFoundMessage("First item missing"),
            new NotFoundMessage("Second item missing"),
        };

        var result = new Result(ResultStatus.NotFound, messages);
        var problem = ProblemResponse.FromResult(result);

        problem.Detail.ShouldBe("First item missing; Second item missing");
    }

    [Fact]
    public void FromResult_SkipsEmptyMessageStrings_InJoin()
    {
        // A message with no text should not contribute a blank segment.
        var messages = new MessageBase[]
        {
            new FailureMessage("Only non-empty"),
            new FailureMessage(),                  // Message = ""
        };

        var result = new Result(ResultStatus.Failure, messages);
        var problem = ProblemResponse.FromResult(result);

        problem.Detail.ShouldBe("Only non-empty");
    }

    // -------------------------------------------------------------------------
    // Messages projection
    // -------------------------------------------------------------------------

    [Fact]
    public void FromResult_ProjectsMessagesToErrorMessage()
    {
        var messages = new MessageBase[]
        {
            new InvalidRequestMessage("Field X is required", "field_required"),
        };

        var result = new Result(ResultStatus.Invalid, messages);
        var problem = ProblemResponse.FromResult(result);

        problem.Messages.Count.ShouldBe(1);
        problem.Messages[0].ShouldSatisfyAllConditions(
            () => problem.Messages[0].Message.ShouldBe("Field X is required"),
            () => problem.Messages[0].Code.ShouldBe("field_required")
        );
    }

    [Fact]
    public void FromResult_Messages_IsEmptyList_WhenNoMessages()
    {
        var result = new Result(ResultStatus.Failure, []);
        var problem = ProblemResponse.FromResult(result);

        problem.Messages.ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Null guard
    // -------------------------------------------------------------------------

    [Fact]
    public void FromResult_ThrowsArgumentNullException_WhenResultIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => ProblemResponse.FromResult(null!));
    }
}
