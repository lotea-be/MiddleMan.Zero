using System.Text.Json;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using MiddleMan.Zero.Abstractions;
using MiddleMan.Zero.AspNetCore.Http;
using MiddleMan.Zero.AspNetCore.Mvc;

namespace MiddleMan.Zero.Tests;

/// <summary>
/// Cross-package lockstep tests that prove the Http and Mvc mappers produce byte-identical
/// <c>application/problem+json</c> bodies for every non-success <see cref="ResultStatus"/>.
/// Both mappers delegate to <see cref="ProblemResponse.FromResult"/> via the shared factory,
/// so byte identity is a strong correctness signal.
/// </summary>
public class LockstepHttpMvcTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private const string ProblemJsonContentType = "application/problem+json";

    /// <summary>
    /// Every non-success <see cref="ResultStatus"/> value paired with a representative
    /// <see cref="MessageBase"/> so the Detail field is populated from real messages.
    /// </summary>
    public static TheoryData<ResultStatus, MessageBase> NonSuccessStatuses =>
        new()
        {
            { ResultStatus.Invalid,   new InvalidRequestMessage("Field is required", "field_required") },
            { ResultStatus.Forbidden, new ForbiddenMessage("Access denied", "access_denied") },
            { ResultStatus.NotFound,  new NotFoundMessage("Item not found", "item_not_found") },
            { ResultStatus.Conflict,  new ConflictMessage("Already exists", "already_exists") },
            { ResultStatus.Failure,   new FailureMessage("Server error", "server_error") },
            { ResultStatus.Undefined, new FailureMessage("Undefined status", "undefined") },
        };

    /// <summary>
    /// For each non-success status: both mappers must produce a byte-identical UTF-8 JSON body
    /// and both must carry <c>Content-Type: application/problem+json</c>.
    /// </summary>
    [Theory]
    [MemberData(nameof(NonSuccessStatuses))]
    public void HttpAndMvcMappers_ProduceBytIdenticalProblemJsonBody(ResultStatus status, MessageBase message)
    {
        // Arrange -- one shared ResultBase fed to both mappers
        var result = new Result(status, [message]);

        // Act -- Http mapper
        var httpIResult = result.ToResult();
        var httpJsonResult = httpIResult.ShouldBeOfType<JsonHttpResult<ProblemResponse>>();

        // Act -- Mvc mapper
        var mvcIActionResult = result.ToActionResult();
        var mvcObjectResult = mvcIActionResult.ShouldBeOfType<ObjectResult>();

        // Extract DTOs
        var httpBody = httpJsonResult.Value;
        var mvcBody = mvcObjectResult.Value.ShouldBeOfType<ProblemResponse>();

        // Serialize both with the SAME options to UTF-8 bytes and compare
        var httpBytes = JsonSerializer.SerializeToUtf8Bytes(httpBody, SerializerOptions);
        var mvcBytes = JsonSerializer.SerializeToUtf8Bytes(mvcBody, SerializerOptions);

        httpBytes.ShouldBe(mvcBytes);

        // Content-type: Http mapper
        httpJsonResult.ContentType.ShouldBe(ProblemJsonContentType);

        // Content-type: Mvc mapper
        mvcObjectResult.ContentTypes.ShouldContain(ProblemJsonContentType);
    }

    /// <summary>
    /// Sanity check: <see cref="ProblemResponse.FromResult"/> throws for <see cref="ResultStatus.Successful"/>.
    /// This guards the factory contract that the lockstep theory relies on.
    /// </summary>
    [Fact]
    public void FromResult_ThrowsInvalidOperationException_WhenResultIsSuccessful()
    {
        var result = new Result(ResultStatus.Successful, []);

        Should.Throw<InvalidOperationException>(
            () => ProblemResponse.FromResult(result));
    }
}
