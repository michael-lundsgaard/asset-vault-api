using AssetVault.Application.Assets.Commands;
using AssetVault.Domain.Events;

namespace AssetVault.UnitTests.Application.Assets.Commands;

public class ConfirmThumbnailUploadCommandHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly ConfirmThumbnailUploadCommandHandler _sut;

    public ConfirmThumbnailUploadCommandHandlerTests() =>
        _sut = new ConfirmThumbnailUploadCommandHandler(_assetRepository, _storageService);

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new ConfirmThumbnailUploadCommand(Guid.NewGuid());
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldSetThumbnailUrlOnAsset()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var command = new ConfirmThumbnailUploadCommand(asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);
        _storageService.GetPublicUrl(Arg.Any<string>()).Returns(x => $"https://cdn.example.com/{x.Arg<string>()}");

        await _sut.Handle(command, CancellationToken.None);

        asset.ThumbnailUrl.Should().Be($"https://cdn.example.com/thumbnails/{asset.Id}/thumbnail");
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldRaiseAssetThumbnailSetEvent()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        asset.ClearDomainEvents();
        var command = new ConfirmThumbnailUploadCommand(asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);
        _storageService.GetPublicUrl(Arg.Any<string>()).Returns("https://cdn.example.com/thumbnails/x/thumbnail");

        await _sut.Handle(command, CancellationToken.None);

        asset.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AssetThumbnailSetEvent>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldSaveChangesAndReturnResponseWithThumbnailUrl()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var command = new ConfirmThumbnailUploadCommand(asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);
        _storageService.GetPublicUrl(Arg.Any<string>()).Returns("https://cdn.example.com/thumbnails/x/thumbnail");

        var result = await _sut.Handle(command, CancellationToken.None);

        await _assetRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.ThumbnailUrl.Should().Be("https://cdn.example.com/thumbnails/x/thumbnail");
    }
}
