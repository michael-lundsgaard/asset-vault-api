using AssetVault.Application.Collections.Commands;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class DeleteCollectionCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly DeleteCollectionCommandHandler _sut;

    public DeleteCollectionCommandHandlerTests() =>
        _sut = new DeleteCollectionCommandHandler(_collectionRepository, _storageService);

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new DeleteCollectionCommand(Guid.NewGuid(), Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenWrongOwner_ShouldThrowUnauthorizedAccessException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "My Collection");
        var command = new DeleteCollectionCommand(Guid.NewGuid(), collection.Id); // different owner
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldDeleteAndSaveChanges()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "My Collection");
        var command = new DeleteCollectionCommand(userId, collection.Id);
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        await _sut.Handle(command, CancellationToken.None);

        await _collectionRepository.Received(1).DeleteAsync(collection, Arg.Any<CancellationToken>());
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
