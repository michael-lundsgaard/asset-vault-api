using System.Net.Http.Headers;
using AssetVault.IntegrationTests.Infrastructure;

namespace AssetVault.IntegrationTests;

/// <summary>
/// Base class for all integration tests. Wires together the two levels of xUnit's test lifecycle:
/// <list type="bullet">
///   <item>
///     <see cref="AssetVaultWebAppFactory"/> (<c>IClassFixture</c>) — one container per test class.
///   </item>
///   <item>
///     <see cref="InitializeAsync"/> (<c>IAsyncLifetime</c>) — database reset per test method.
///     xUnit instantiates a new subclass for every test, so <see cref="InitializeAsync"/> runs once
///     per test before the test body executes.
///   </item>
/// </list>
/// </summary>
public abstract class IntegrationTestBase(AssetVaultWebAppFactory factory) : IClassFixture<AssetVaultWebAppFactory>, IAsyncLifetime
{
    /// <summary>Pre-configured HTTP client pointing at the in-process test server.</summary>
    protected readonly HttpClient Client = factory.CreateClient();

    /// <summary>
    /// Runs before each test method. Resets the database to a clean post-migration state
    /// so tests are fully isolated from one another.
    /// </summary>
    public async Task InitializeAsync() => await factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Sets the <c>Authorization: Test {userId}:{email}</c> header on <see cref="Client"/>.
    /// Call again mid-test to switch identities (e.g. to verify a 403 for a different owner).
    /// </summary>
    protected void AuthenticateAs(Guid userId, string email) =>
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.SchemeName, $"{userId}:{email}");
}
