using System.Net;
using System.Net.Http.Json;
using AssetVault.Contracts.Requests.Assets;
using AssetVault.Contracts.Requests.Collections;
using AssetVault.Contracts.Responses.Assets;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Contracts.Responses.Common;
using AssetVault.IntegrationTests.Infrastructure;

namespace AssetVault.IntegrationTests.Collections;

public class CollectionsIntegrationTests(AssetVaultWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateCollection_GivenValidRequest_ShouldReturn201WithCollectionResponse()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.PostAsJsonAsync("api/collections",
            new CreateCollectionRequest("Travel Photos", "Summer 2025"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        body!.Name.Should().Be("Travel Photos");
        body.Description.Should().Be("Summer 2025");
    }

    [Fact]
    public async Task CreateCollection_GivenUnauthenticatedRequest_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("api/collections",
            new CreateCollectionRequest("Test"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCollectionById_GivenExistingCollection_ShouldReturn200()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");
        var collectionId = await CreateCollectionAndGetId("My Albums");

        var response = await Client.GetAsync($"api/collections/{collectionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        body!.Id.Should().Be(collectionId);
        body.Name.Should().Be("My Albums");
    }

    [Fact]
    public async Task GetCollectionById_GivenUnknownId_ShouldReturn404()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.GetAsync($"api/collections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCollection_GivenValidRequest_ShouldReturn200WithUpdatedResponse()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var collectionId = await CreateCollectionAndGetId("Original Name");

        var response = await Client.PatchAsJsonAsync($"api/collections/{collectionId}",
            new UpdateCollectionRequest("Updated Name", "New description"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        body!.Name.Should().Be("Updated Name");
        body.Description.Should().Be("New description");
    }

    [Fact]
    public async Task UpdateCollection_GivenWrongOwner_ShouldReturn403()
    {
        AuthenticateAs(Guid.NewGuid(), "owner@example.com");
        var collectionId = await CreateCollectionAndGetId("Owned Collection");

        AuthenticateAs(Guid.NewGuid(), "attacker@example.com");
        var response = await Client.PatchAsJsonAsync($"api/collections/{collectionId}",
            new UpdateCollectionRequest("Hijacked"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCollection_GivenUnknownId_ShouldReturn404()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.PatchAsJsonAsync($"api/collections/{Guid.NewGuid()}",
            new UpdateCollectionRequest("Name"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCollection_GivenValidId_ShouldReturn204()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var collectionId = await CreateCollectionAndGetId("To Be Deleted");

        var response = await Client.DeleteAsync($"api/collections/{collectionId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCollection_GivenUnknownId_ShouldReturn404()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.DeleteAsync($"api/collections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCollections_ShouldReturn200WithPaginatedResponse()
    {
        AuthenticateAs(Guid.NewGuid(), "user@example.com");

        var response = await Client.GetAsync("api/collections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<CollectionResponse>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAssetToCollection_GivenValidIds_ShouldReturn204()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var collectionId = await CreateCollectionAndGetId("My Collection");
        var assetId = await InitiateUploadAndGetId("photo.jpg");

        var response = await Client.PostAsync($"api/collections/{collectionId}/assets/{assetId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddAssetToCollection_GivenAssetAlreadyAdded_ShouldReturn204Idempotently()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var collectionId = await CreateCollectionAndGetId("My Collection");
        var assetId = await InitiateUploadAndGetId("photo.jpg");
        await Client.PostAsync($"api/collections/{collectionId}/assets/{assetId}", null);

        // Second add should be idempotent
        var response = await Client.PostAsync($"api/collections/{collectionId}/assets/{assetId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveAssetFromCollection_GivenValidIds_ShouldReturn204()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, "user@example.com");
        var collectionId = await CreateCollectionAndGetId("My Collection");
        var assetId = await InitiateUploadAndGetId("photo.jpg");
        await Client.PostAsync($"api/collections/{collectionId}/assets/{assetId}", null);

        var response = await Client.DeleteAsync($"api/collections/{collectionId}/assets/{assetId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<Guid> CreateCollectionAndGetId(string name)
    {
        var response = await Client.PostAsJsonAsync("api/collections",
            new CreateCollectionRequest(name));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        return result!.Id;
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
