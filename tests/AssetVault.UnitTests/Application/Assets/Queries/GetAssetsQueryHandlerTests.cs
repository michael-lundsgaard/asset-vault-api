using AssetVault.Application.Assets.Queries;
using AssetVault.Application.Common;

namespace AssetVault.UnitTests.Application.Assets.Queries;

public class GetAssetsQueryHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly GetAssetsQueryHandler _sut;

    public GetAssetsQueryHandlerTests() =>
        _sut = new GetAssetsQueryHandler(_assetRepository);

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponse()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "img.png", "image/png", 512);
        var pagedResult = new PagedResult<MediaAsset>([asset], 1, 1, 20);
        var query = new GetAssetsQuery(new AssetQuery());
        _assetRepository.GetPagedAsync(query.Query, Arg.Any<CancellationToken>()).Returns(pagedResult);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }
}
