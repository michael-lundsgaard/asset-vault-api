using AssetVault.Application.Collections.Commands;

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
    public async Task Handle_GivenWrongOwner_ShouldThrowUnauthorizedAccessException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Original");
        var command = new UpdateCollectionCommand(Guid.NewGuid(), collection.Id, "New Name", null); // different owner
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldUpdateCollectionAndSaveChanges()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Old Name");
        var command = new UpdateCollectionCommand(userId, collection.Id, "New Name", "New desc");
        _collectionRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New desc");
        await _collectionRepository.Received(1).UpdateAsync(collection, Arg.Any<CancellationToken>());
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
