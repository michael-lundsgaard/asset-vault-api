using AssetVault.Application.Collections.Queries;
using AssetVault.Application.Common;

namespace AssetVault.UnitTests.Application.Collections.Queries;

public class GetCollectionsByUserQueryHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly GetCollectionsByUserQueryHandler _sut;

    public GetCollectionsByUserQueryHandlerTests() =>
        _sut = new GetCollectionsByUserQueryHandler(_collectionRepository);

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponseForUser()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Private");
        var pagedResult = new PagedResult<Collection>([collection], 1, 1, 20);
        var query = new GetCollectionsByUserQuery(userId, new CollectionQuery());
        _collectionRepository.GetPagedByUserAsync(userId, query.Query, Arg.Any<CancellationToken>()).Returns(pagedResult);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }
}
