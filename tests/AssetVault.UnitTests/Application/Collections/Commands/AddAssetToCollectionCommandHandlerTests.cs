using AssetVault.Application.Collections.Commands;
using AssetVault.Domain.Enums;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class AddAssetToCollectionCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly AddAssetToCollectionCommandHandler _sut;

    public AddAssetToCollectionCommandHandlerTests() =>
        _sut = new AddAssetToCollectionCommandHandler(_collectionRepository, _assetRepository);

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenPrivateCollectionAndWrongOwner_ShouldThrowUnauthorizedAccessException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Private", null, CollectionType.Private);
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), collection.Id, Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenSharedCollectionAndDifferentUser_ShouldNotThrow()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Shared");
        var asset = MediaAsset.Create(Guid.NewGuid(), "file.jpg", "image/jpeg", 512);
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), collection.Id, asset.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        asset.Collections.Should().ContainSingle(c => c.Id == collection.Id);
    }

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Shared");
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), collection.Id, Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenAssetAlreadyInCollection_ShouldReturnWithoutSaving()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Shared");
        var asset = MediaAsset.Create(Guid.NewGuid(), "file.jpg", "image/jpeg", 512);
        asset.AddToCollection(collection);
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), collection.Id, asset.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        await _assetRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldAddAssetToCollectionAndSaveChanges()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Shared");
        var asset = MediaAsset.Create(Guid.NewGuid(), "file.jpg", "image/jpeg", 512);
        var command = new AddAssetToCollectionCommand(Guid.NewGuid(), collection.Id, asset.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _assetRepository.GetByIdAsync(command.AssetId, AssetExpand.Collections, Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        asset.Collections.Should().ContainSingle(c => c.Id == collection.Id);
        await _assetRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
