using AssetVault.Application.Collections.Commands;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class InitiateCoverUploadCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly InitiateCoverUploadCommandHandler _sut;

    public InitiateCoverUploadCommandHandlerTests() =>
        _sut = new InitiateCoverUploadCommandHandler(_collectionRepository, _storageService);

    [Fact]
    public async Task Handle_GivenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new InitiateCoverUploadCommand(Guid.NewGuid(), Guid.NewGuid(), "image/jpeg", 1024);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenWrongOwner_ShouldThrowUnauthorizedAccessException()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Nature");
        var command = new InitiateCoverUploadCommand(Guid.NewGuid(), collection.Id, "image/jpeg", 1024);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldReturnPresignedUrl()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Nature");
        var expiry = DateTime.UtcNow.AddMinutes(15);
        var command = new InitiateCoverUploadCommand(userId, collection.Id, "image/png", 2048);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _storageService
            .GenerateCoverImageUploadUrlAsync(command.CollectionId, command.ContentType, Arg.Any<CancellationToken>())
            .Returns(new PresignedUploadResult("https://s3.example.com/cover", $"covers/{collection.Id}/cover", expiry));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.PresignedUrl.Should().Be("https://s3.example.com/cover");
        result.ExpiresAt.Should().Be(expiry);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldNotWriteToDatabase()
    {
        var userId = Guid.NewGuid();
        var collection = Collection.Create(userId, "Nature");
        var command = new InitiateCoverUploadCommand(userId, collection.Id, "image/png", 2048);
        _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken: Arg.Any<CancellationToken>()).Returns(collection);
        _storageService
            .GenerateCoverImageUploadUrlAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PresignedUploadResult("https://s3.example.com/cover", "covers/x/cover", DateTime.UtcNow.AddMinutes(15)));

        await _sut.Handle(command, CancellationToken.None);

        await _collectionRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
