using AssetVault.Application.Assets.Commands;
using AssetVault.Domain.Events;

namespace AssetVault.UnitTests.Application.Assets.Commands;

public class DeleteAssetThumbnailCommandHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly DeleteAssetThumbnailCommandHandler _sut;

    public DeleteAssetThumbnailCommandHandlerTests() =>
        _sut = new DeleteAssetThumbnailCommandHandler(_assetRepository, _storageService);

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new DeleteAssetThumbnailCommand(Guid.NewGuid());
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenAssetWithNoThumbnail_ShouldDoNothing()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var command = new DeleteAssetThumbnailCommand(asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        await _storageService.DidNotReceive().DeletePublicAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _assetRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenAssetWithThumbnail_ShouldDeleteFromStorageAndClearUrl()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        asset.SetThumbnailUrl($"https://cdn.example.com/thumbnails/{asset.Id}/thumbnail");
        asset.ClearDomainEvents();
        var command = new DeleteAssetThumbnailCommand(asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        await _storageService.Received(1).DeletePublicAsync($"thumbnails/{asset.Id}/thumbnail", Arg.Any<CancellationToken>());
        asset.ThumbnailUrl.Should().BeNull();
        await _assetRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenAssetWithThumbnail_ShouldRaiseAssetThumbnailRemovedEvent()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        asset.SetThumbnailUrl($"https://cdn.example.com/thumbnails/{asset.Id}/thumbnail");
        asset.ClearDomainEvents();
        var command = new DeleteAssetThumbnailCommand(asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        asset.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AssetThumbnailRemovedEvent>();
    }
}
