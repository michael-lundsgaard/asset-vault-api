using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AssetVault.IntegrationTests.Infrastructure;

/// <summary>
/// Bypasses Supabase JWT validation in integration tests.
/// Set the header via <see cref="IntegrationTestBase.AuthenticateAs"/> before each request.
/// Format: <c>Authorization: Test {userId}:{email}</c>
/// </summary>
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var value = authHeader.ToString();
        if (!value.StartsWith($"{SchemeName} "))
            return Task.FromResult(AuthenticateResult.NoResult());

        var payload = value[$"{SchemeName} ".Length..];
        var separatorIndex = payload.IndexOf(':');
        if (separatorIndex < 0)
            return Task.FromResult(AuthenticateResult.Fail("Invalid test auth format. Expected: Test {userId}:{email}"));

        var userId = payload[..separatorIndex];
        var email = payload[(separatorIndex + 1)..];

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
