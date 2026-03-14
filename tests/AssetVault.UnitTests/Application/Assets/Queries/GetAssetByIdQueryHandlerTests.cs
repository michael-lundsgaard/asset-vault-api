using AssetVault.Application.Assets.Queries;

namespace AssetVault.UnitTests.Application.Assets.Queries;

public class GetAssetByIdQueryHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly GetAssetByIdQueryHandler _sut;

    public GetAssetByIdQueryHandlerTests() =>
        _sut = new GetAssetByIdQueryHandler(_assetRepository);

    [Fact]
    public async Task Handle_GivenAssetExists_ShouldReturnAssetResponse()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "photo.jpg", "image/jpeg", 1024);
        var query = new GetAssetByIdQuery(asset.Id);
        _assetRepository.GetByIdAsync(asset.Id, AssetExpand.None, Arg.Any<CancellationToken>()).Returns(asset);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(asset.Id);
        result.FileName.Should().Be("photo.jpg");
    }

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldReturnNull()
    {
        var query = new GetAssetByIdQuery(Guid.NewGuid());
        _assetRepository.GetByIdAsync(query.Id, AssetExpand.None, Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenExpandCollections_ShouldPassExpandFlagToRepository()
    {
        var assetId = Guid.NewGuid();
        var query = new GetAssetByIdQuery(assetId, AssetExpand.Collections);
        _assetRepository.GetByIdAsync(assetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        await _sut.Handle(query, CancellationToken.None);

        await _assetRepository.Received(1).GetByIdAsync(assetId, AssetExpand.Collections, Arg.Any<CancellationToken>());
    }
}
