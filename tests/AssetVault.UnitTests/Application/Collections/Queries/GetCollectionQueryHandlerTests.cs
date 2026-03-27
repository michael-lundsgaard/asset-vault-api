using AssetVault.Application.Collections.Queries;
using AssetVault.Domain.Enums;

namespace AssetVault.UnitTests.Application.Collections.Queries;

public class GetCollectionByIdQueryHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly GetCollectionByIdQueryHandler _sut;

    public GetCollectionByIdQueryHandlerTests() =>
        _sut = new GetCollectionByIdQueryHandler(_collectionRepository);

    [Fact]
    public async Task Handle_GivenSharedCollection_ShouldReturnForAnyUser()
    {
        var collection = Collection.Create(Guid.NewGuid(), "My Collection", "desc");
        var query = new GetCollectionByIdQuery(collection.Id, CollectionExpand.None, Guid.NewGuid());
        _collectionRepository.GetByIdAsync(collection.Id, CollectionExpand.None, Arg.Any<CancellationToken>()).Returns(collection);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(collection.Id);
    }

    [Fact]
    public async Task Handle_GivenPrivateCollection_WhenRequestedByOwner_ShouldReturnCollection()
    {
        var ownerId = Guid.NewGuid();
        var collection = Collection.Create(ownerId, "Private", null, CollectionType.Private);
        var query = new GetCollectionByIdQuery(collection.Id, CollectionExpand.None, ownerId);
        _collectionRepository.GetByIdAsync(collection.Id, CollectionExpand.None, Arg.Any<CancellationToken>()).Returns(collection);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_GivenPrivateCollection_WhenRequestedByNonOwner_ShouldReturnNull()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Private", null, CollectionType.Private);
        var query = new GetCollectionByIdQuery(collection.Id, CollectionExpand.None, Guid.NewGuid());
        _collectionRepository.GetByIdAsync(collection.Id, CollectionExpand.None, Arg.Any<CancellationToken>()).Returns(collection);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenFavoritesCollection_WhenRequestedByNonOwner_ShouldReturnNull()
    {
        var ownerId = Guid.NewGuid();
        var favorites = Collection.CreateFavorites(ownerId);
        var query = new GetCollectionByIdQuery(favorites.Id, CollectionExpand.None, Guid.NewGuid());
        _collectionRepository.GetByIdAsync(favorites.Id, CollectionExpand.None, Arg.Any<CancellationToken>()).Returns(favorites);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldReturnNull()
    {
        var query = new GetCollectionByIdQuery(Guid.NewGuid());
        _collectionRepository.GetByIdAsync(query.Id, CollectionExpand.None, Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenExpandAssets_ShouldPassExpandFlagToRepository()
    {
        var collectionId = Guid.NewGuid();
        var query = new GetCollectionByIdQuery(collectionId, CollectionExpand.Assets);
        _collectionRepository.GetByIdAsync(collectionId, CollectionExpand.Assets, Arg.Any<CancellationToken>()).Returns((Collection?)null);

        await _sut.Handle(query, CancellationToken.None);

        await _collectionRepository.Received(1).GetByIdAsync(collectionId, CollectionExpand.Assets, Arg.Any<CancellationToken>());
    }
}
