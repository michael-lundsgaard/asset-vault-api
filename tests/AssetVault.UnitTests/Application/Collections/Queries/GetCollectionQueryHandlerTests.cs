using AssetVault.Application.Collections.Queries;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace AssetVault.UnitTests.Application.Collections.Queries;

public class GetCollectionByIdQueryHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly GetCollectionByIdQueryHandler _sut;

    public GetCollectionByIdQueryHandlerTests() =>
        _sut = new GetCollectionByIdQueryHandler(_collectionRepository);

    [Fact]
    public async Task Handle_GivenCollectionExists_ShouldReturnCollectionResponse()
    {
        var collection = Collection.Create(Guid.NewGuid(), "My Collection", "desc");
        var query = new GetCollectionByIdQuery(collection.Id);
        _collectionRepository.GetByIdAsync(collection.Id, CollectionExpand.None, Arg.Any<CancellationToken>()).Returns(collection);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(collection.Id);
        result.Name.Should().Be("My Collection");
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
