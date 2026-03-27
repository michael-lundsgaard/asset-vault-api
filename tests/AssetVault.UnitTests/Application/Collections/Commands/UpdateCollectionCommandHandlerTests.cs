using AssetVault.Application.Collections.Commands;
using AssetVault.Domain.Enums;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class UpdateCollectionCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly UpdateCollectionCommandHandler _sut;

    public UpdateCollectionCommandHandlerTests() =>
        _sut = new UpdateCollectionCommandHandler(_collectionRepository);

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new UpdateCollectionCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name", null);
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenFavoritesCollection_ShouldThrowInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var favorites = Collection.CreateFavorites(userId);
        var command = new UpdateCollectionCommand(userId, favorites.Id, "New Name", null);
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(favorites);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_GivenPrivateCollectionAndWrongOwner_ShouldThrowUnauthorizedAccessException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Private", null, CollectionType.Private);
        var command = new UpdateCollectionCommand(Guid.NewGuid(), collection.Id, "New Name", null);
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenSharedCollectionAndDifferentUser_ShouldUpdateSuccessfully()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Shared");
        var differentUserId = Guid.NewGuid();
        var command = new UpdateCollectionCommand(differentUserId, collection.Id, "New Name", "desc");
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        await _collectionRepository.Received(1).UpdateAsync(collection, Arg.Any<CancellationToken>());
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenPrivateCollectionAndCorrectOwner_ShouldUpdateSuccessfully()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Private", null, CollectionType.Private);
        var command = new UpdateCollectionCommand(userId, collection.Id, "New Name", "desc");
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
