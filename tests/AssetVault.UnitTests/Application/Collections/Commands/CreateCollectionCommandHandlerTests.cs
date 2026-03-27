using AssetVault.Application.Collections.Commands;
using AssetVault.Domain.Enums;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class CreateCollectionCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly CreateCollectionCommandHandler _sut;

    public CreateCollectionCommandHandlerTests() =>
        _sut = new CreateCollectionCommandHandler(_collectionRepository);

    [Fact]
    public async Task Handle_GivenFavoritesType_ShouldThrowInvalidOperationException()
    {
        var command = new CreateCollectionCommand(Guid.NewGuid(), "Favorites", null, CollectionType.Favorites);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _collectionRepository.DidNotReceive().AddAsync(Arg.Any<Collection>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldReturnCollectionResponse()
    {
        var command = new CreateCollectionCommand(Guid.NewGuid(), "Travel", "My travel photos");
        _collectionRepository
            .AddAsync(Arg.Any<Collection>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Travel");
        result.UserId.Should().Be(command.UserId);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldAddAndSaveChanges()
    {
        var command = new CreateCollectionCommand(Guid.NewGuid(), "Travel", null);

        await _sut.Handle(command, CancellationToken.None);

        await _collectionRepository.Received(1).AddAsync(Arg.Any<Collection>(), Arg.Any<CancellationToken>());
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
