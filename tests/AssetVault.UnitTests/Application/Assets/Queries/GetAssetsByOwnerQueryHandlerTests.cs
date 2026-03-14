using AssetVault.Application.Assets.Queries;
using AssetVault.Application.Common;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace AssetVault.UnitTests.Application.Assets.Queries;

public class GetAssetsByOwnerQueryHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly GetAssetsByOwnerQueryHandler _sut;

    public GetAssetsByOwnerQueryHandlerTests() =>
        _sut = new GetAssetsByOwnerQueryHandler(_assetRepository);

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponseForOwner()
    {
        var userId = Guid.NewGuid();
        var asset = MediaAsset.Create(userId, "img.png", "image/png", 512);
        var pagedResult = new PagedResult<MediaAsset>([asset], 1, 1, 20);
        var query = new GetAssetsByOwnerQuery(userId, new AssetQuery());
        _assetRepository.GetPagedByUserAsync(userId, query.Query, Arg.Any<CancellationToken>()).Returns(pagedResult);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }
}
