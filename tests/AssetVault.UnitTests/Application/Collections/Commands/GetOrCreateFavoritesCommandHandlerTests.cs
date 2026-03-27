using AssetVault.Application.Collections.Commands;

namespace AssetVault.UnitTests.Application.Collections.Commands;

public class EnsureFavoritesCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly EnsureFavoritesCommandHandler _sut;

    public EnsureFavoritesCommandHandlerTests() =>
        _sut = new EnsureFavoritesCommandHandler(_collectionRepository);

    [Fact]
    public async Task Handle_GivenFavoritesExist_ShouldReturnExistingCollection()
    {
        var userId = Guid.NewGuid();
        var existing = Collection.CreateFavorites(userId);
        var command = new EnsureFavoritesCommand(userId);
        _collectionRepository.GetFavoritesAsync(userId, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Id.Should().Be(existing.Id);
        result.Type.Should().Be("Favorites");
        await _collectionRepository.DidNotReceive().AddAsync(Arg.Any<Collection>(), Arg.Any<CancellationToken>());
        await _collectionRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNoFavoritesExist_ShouldCreateAndReturnFavoritesCollection()
    {
        var userId = Guid.NewGuid();
        var command = new EnsureFavoritesCommand(userId);
        _collectionRepository.GetFavoritesAsync(userId, Arg.Any<CancellationToken>()).Returns((Collection?)null);

        Collection? created = null;
        await _collectionRepository.AddAsync(Arg.Do<Collection>(c => created = c), Arg.Any<CancellationToken>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Type.Should().Be("Favorites");
        result.Name.Should().Be("Favorites");
        result.UserId.Should().Be(userId);
        await _collectionRepository.Received(1).AddAsync(Arg.Any<Collection>(), Arg.Any<CancellationToken>());
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
