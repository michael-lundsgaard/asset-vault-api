using AssetVault.Application.Collections.Commands;
using AssetVault.Domain.Events;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class DeleteCollectionCoverCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly DeleteCollectionCoverCommandHandler _sut;

    public DeleteCollectionCoverCommandHandlerTests() =>
        _sut = new DeleteCollectionCoverCommandHandler(_collectionRepository, _storageService);

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new DeleteCollectionCoverCommand(Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenCollectionWithNoCover_ShouldDoNothing()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Nature");
        var command = new DeleteCollectionCoverCommand(collection.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        await _sut.Handle(command, CancellationToken.None);

        await _storageService.DidNotReceive().DeletePublicAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _collectionRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenCollectionWithCover_ShouldDeleteFromStorageAndClearUrl()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Nature");
        collection.SetCoverImageUrl($"https://cdn.example.com/covers/{collection.Id}/cover");
        var command = new DeleteCollectionCoverCommand(collection.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        await _sut.Handle(command, CancellationToken.None);

        await _storageService.Received(1).DeletePublicAsync($"covers/{collection.Id}/cover", Arg.Any<CancellationToken>());
        collection.CoverImageUrl.Should().BeNull();
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenCollectionWithCover_ShouldRaiseCollectionCoverRemovedEvent()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Nature");
        collection.SetCoverImageUrl($"https://cdn.example.com/covers/{collection.Id}/cover");
        collection.ClearDomainEvents();
        var command = new DeleteCollectionCoverCommand(collection.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        await _sut.Handle(command, CancellationToken.None);

        collection.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CollectionCoverRemovedEvent>();
    }
}
