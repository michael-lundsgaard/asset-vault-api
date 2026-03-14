using System.Net.Http.Headers;
using AssetVault.IntegrationTests.Infrastructure;

namespace AssetVault.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<AssetVaultWebAppFactory>
{
    protected readonly HttpClient Client;

    protected IntegrationTestBase(AssetVaultWebAppFactory factory) =>
        Client = factory.CreateClient();

    // Creates an authenticated client for the given user. UserId and email are used by TestAuthHandler.
    protected void AuthenticateAs(Guid userId, string email) =>
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.SchemeName, $"{userId}:{email}");
}
