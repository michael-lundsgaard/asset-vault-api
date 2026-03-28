using AssetVault.Application.Collections.Commands;
using AssetVault.Domain.Events;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class ConfirmCoverUploadCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly ConfirmCoverUploadCommandHandler _sut;

    public ConfirmCoverUploadCommandHandlerTests() =>
        _sut = new ConfirmCoverUploadCommandHandler(_collectionRepository, _storageService);

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new ConfirmCoverUploadCommand(Guid.NewGuid(), Guid.NewGuid());
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenWrongOwner_ShouldThrowUnauthorizedAccessException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Nature");
        var command = new ConfirmCoverUploadCommand(Guid.NewGuid(), collection.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldSetCoverImageUrl()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Nature");
        var command = new ConfirmCoverUploadCommand(userId, collection.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _storageService.GetPublicUrl(Arg.Any<string>()).Returns(x => $"https://cdn.example.com/{x.Arg<string>()}");

        await _sut.Handle(command, CancellationToken.None);

        collection.CoverImageUrl.Should().Be($"https://cdn.example.com/covers/{collection.Id}/cover");
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldRaiseCollectionCoverSetEvent()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Nature");
        var command = new ConfirmCoverUploadCommand(userId, collection.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _storageService.GetPublicUrl(Arg.Any<string>()).Returns("https://cdn.example.com/covers/x/cover");

        await _sut.Handle(command, CancellationToken.None);

        collection.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CollectionCoverSetEvent>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldSaveChangesAndReturnResponseWithCoverImageUrl()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Nature");
        var command = new ConfirmCoverUploadCommand(userId, collection.Id);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _storageService.GetPublicUrl(Arg.Any<string>()).Returns("https://cdn.example.com/covers/x/cover");

        var result = await _sut.Handle(command, CancellationToken.None);

        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.CoverImageUrl.Should().Be("https://cdn.example.com/covers/x/cover");
    }
}
