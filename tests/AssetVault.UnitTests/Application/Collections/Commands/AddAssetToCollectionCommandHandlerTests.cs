using AssetVault.Application.Collections.Commands;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class AddAssetToCollectionCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly AddAssetToCollectionCommandHandler _sut;

    public AddAssetToCollectionCommandHandlerTests() =>
        _sut = new AddAssetToCollectionCommandHandler(_collectionRepository, _assetRepository);

    private static (MediaAsset asset, Collection collection) CreateMatchingPair()
    {
        var userId = Guid.NewGuid();
        var asset = MediaAsset.Create(userId, "file.jpg", "image/jpeg", 512);
        var collection = Collection.Create(userId, "My Collection");
        return (asset, collection);
    }

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenWrongCollectionOwner_ShouldThrowUnauthorizedAccessException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Coll");
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), collection.Id, Guid.NewGuid()); // different owner
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Coll");
        var command = new AddAssetToCollectionCommand(userId, collection.Id, Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenWrongAssetOwner_ShouldThrowUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Coll");
        var asset = MediaAsset.Create(Guid.NewGuid(), "f.jpg", "image/jpeg", 100); // owned by different user
        var command = new AddAssetToCollectionCommand(userId, collection.Id, asset.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns(asset);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenAssetAlreadyInCollection_ShouldReturnWithoutSaving()
    {
        var (asset, collection) = CreateMatchingPair();
        asset.AddToCollection(collection); // pre-add so it's already there
        var command = new AddAssetToCollectionCommand(asset.UserId, collection.Id, asset.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        await _assetRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldAddAssetToCollectionAndSaveChanges()
    {
        var (asset, collection) = CreateMatchingPair();
        var command = new AddAssetToCollectionCommand(asset.UserId, collection.Id, asset.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        asset.Collections.Should().ContainSingle(c => c.Id == collection.Id);
        await _assetRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
