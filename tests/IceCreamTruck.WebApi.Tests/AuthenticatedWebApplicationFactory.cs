using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
namespace IceCreamTruck.WebApi.Tests;

/// <summary>
/// A custom WebApplicationFactory that registers a no-op authentication scheme.
/// This enables ForbidResult (HTTP 403) to work correctly in integration tests
/// without a real authentication system.
/// </summary>
public class AuthenticatedWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string SchemeName = "Test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services
                .AddAuthentication(SchemeName)
                .AddScheme<AuthenticationSchemeOptions, NoOpAuthHandler>(SchemeName, _ => { });
        });
    }
}

/// <summary>
/// An authentication handler that always authenticates the request as an anonymous user.
/// Its sole purpose is to satisfy the authentication pipeline so that ForbidResult
/// correctly produces HTTP 403 instead of throwing an exception.
/// </summary>
internal sealed class NoOpAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity([], Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
