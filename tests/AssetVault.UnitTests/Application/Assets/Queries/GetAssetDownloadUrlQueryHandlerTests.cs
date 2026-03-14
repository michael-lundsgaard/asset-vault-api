using AssetVault.Application.Assets.Queries;

namespace AssetVault.UnitTests.Application.Assets.Queries;

public class GetAssetDownloadUrlQueryHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly GetAssetDownloadUrlQueryHandler _sut;

    public GetAssetDownloadUrlQueryHandlerTests() =>
        _sut = new GetAssetDownloadUrlQueryHandler(_assetRepository, _storageService);

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var query = new GetAssetDownloadUrlQuery(Guid.NewGuid());
        _assetRepository.GetByIdAsync(query.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenPendingAsset_ShouldThrowInvalidOperationException()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "file.jpg", "image/jpeg", 1024);
        // Status is Pending by default; no storage path set
        var query = new GetAssetDownloadUrlQuery(asset.Id);
        _assetRepository.GetByIdAsync(query.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_GivenActiveAssetWithNoStoragePath_ShouldThrowInvalidOperationException()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "file.jpg", "image/jpeg", 1024);
        asset.MarkAsUploaded(); // Active but no StoragePath
        var query = new GetAssetDownloadUrlQuery(asset.Id);
        _assetRepository.GetByIdAsync(query.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_GivenActiveAssetWithStoragePath_ShouldReturnDownloadUrl()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "file.jpg", "image/jpeg", 1024);
        asset.SetStoragePath("uploads/id/file.jpg");
        asset.MarkAsUploaded();
        var expiry = DateTime.UtcNow.AddHours(1);
        var query = new GetAssetDownloadUrlQuery(asset.Id);
        _assetRepository.GetByIdAsync(query.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);
        _storageService
            .GenerateDownloadUrlAsync("uploads/id/file.jpg", Arg.Any<CancellationToken>())
            .Returns(new PresignedDownloadResult("https://s3.example.com/download", expiry));

        var result = await _sut.Handle(query, CancellationToken.None);

        result.PresignedUrl.Should().Be("https://s3.example.com/download");
        result.ExpiresAt.Should().Be(expiry);
        result.AssetId.Should().Be(asset.Id);
    }
}
