using AssetVault.Application.Collections.Queries;
using AssetVault.Application.Common;

namespace AssetVault.UnitTests.Application.Collections.Queries;

public class GetCollectionsQueryHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly GetCollectionsQueryHandler _sut;

    public GetCollectionsQueryHandlerTests() =>
        _sut = new GetCollectionsQueryHandler(_collectionRepository);

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponse()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Test");
        var pagedResult = new PagedResult<Collection>([collection], 1, 1, 20);
        var query = new GetCollectionsQuery(new CollectionQuery());
        _collectionRepository.GetPagedAsync(query.Query, Arg.Any<CancellationToken>()).Returns(pagedResult);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }
}
