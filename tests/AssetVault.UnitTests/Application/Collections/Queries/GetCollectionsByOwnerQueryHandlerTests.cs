using AssetVault.Application.Collections.Queries;
using AssetVault.Application.Common;

namespace AssetVault.UnitTests.Application.Collections.Queries;

public class GetCollectionsByOwnerQueryHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly GetCollectionsByOwnerQueryHandler _sut;

    public GetCollectionsByOwnerQueryHandlerTests() =>
        _sut = new GetCollectionsByOwnerQueryHandler(_collectionRepository);

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponseForOwner()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Test");
        var pagedResult = new PagedResult<Collection>([collection], 1, 1, 20);
        var query = new GetCollectionsByOwnerQuery(userId, new CollectionQuery());
        _collectionRepository.GetPagedByUserAsync(userId, query.Query, Arg.Any<CancellationToken>()).Returns(pagedResult);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }
}
