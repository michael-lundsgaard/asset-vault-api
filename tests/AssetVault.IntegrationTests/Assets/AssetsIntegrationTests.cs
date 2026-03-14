using System.Net;
using System.Net.Http.Json;
using AssetVault.Contracts.Requests.Assets;
using AssetVault.Contracts.Responses.Assets;
using AssetVault.Contracts.Responses.Common;
using AssetVault.IntegrationTests.Infrastructure;

namespace AssetVault.IntegrationTests.Assets;

public class AssetsIntegrationTests(AssetVaultWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task InitiateUpload_GivenValidRequest_ShouldReturn201WithPresignedUrl()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.PostAsJsonAsync("api/assets/upload",
            new InitiateUploadRequest("photo.jpg", "image/jpeg", 1024));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PresignedUploadResponse>();
        body.Should().NotBeNull();
        body!.AssetId.Should().NotBeEmpty();
        body.PresignedUrl.Should().StartWith("https://fake-storage.test");
    }

    [Fact]
    public async Task InitiateUpload_GivenUnauthenticatedRequest_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("api/assets/upload",
            new InitiateUploadRequest("photo.jpg", "image/jpeg", 1024));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConfirmUpload_GivenValidAsset_ShouldReturn204()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var assetId = await InitiateUploadAndGetId("file.jpg");

        var response = await Client.PatchAsync($"api/assets/{assetId}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ConfirmUpload_GivenWrongOwner_ShouldReturn403()
    {
        AuthenticateAs(Guid.NewGuid(), "owner@example.com");
        var assetId = await InitiateUploadAndGetId("file.jpg");

        // Switch to a different user
        AuthenticateAs(Guid.NewGuid(), "attacker@example.com");
        var response = await Client.PatchAsync($"api/assets/{assetId}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ConfirmUpload_GivenUnknownAssetId_ShouldReturn404()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.PatchAsync($"api/assets/{Guid.NewGuid()}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAssetById_GivenExistingAsset_ShouldReturn200WithAssetResponse()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var assetId = await InitiateUploadAndGetId("image.png");

        var response = await Client.GetAsync($"api/assets/{assetId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AssetResponse>();
        body!.Id.Should().Be(assetId);
        body.FileName.Should().Be("image.png");
    }

    [Fact]
    public async Task GetAssetById_GivenUnknownId_ShouldReturn404()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.GetAsync($"api/assets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDownloadUrl_GivenActiveAsset_ShouldReturn200WithPresignedUrl()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var assetId = await InitiateUploadAndGetId("video.mp4");
        await Client.PatchAsync($"api/assets/{assetId}/confirm", null);

        var response = await Client.GetAsync($"api/assets/{assetId}/download");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PresignedDownloadResponse>();
        body!.PresignedUrl.Should().StartWith("https://fake-storage.test");
    }

    [Fact]
    public async Task GetDownloadUrl_GivenPendingAsset_ShouldReturn500()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var assetId = await InitiateUploadAndGetId("pending.jpg");
        // Upload is NOT confirmed — status remains Pending

        var response = await Client.GetAsync($"api/assets/{assetId}/download");

        // ExceptionHandlingMiddleware maps InvalidOperationException → 500
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetAssets_ShouldReturn200WithPaginatedResponse()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.GetAsync("api/assets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<AssetResponse>>();
        body.Should().NotBeNull();
    }

    private async Task<Guid> InitiateUploadAndGetId(string fileName)
    {
        var response = await Client.PostAsJsonAsync("api/assets/upload",
            new InitiateUploadRequest(fileName, "application/octet-stream", 1024));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PresignedUploadResponse>();
        return result!.AssetId;
    }
}
