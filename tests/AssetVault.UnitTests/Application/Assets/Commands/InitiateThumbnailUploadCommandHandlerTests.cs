using AssetVault.Application.Assets.Commands;

namespace AssetVault.UnitTests.Application.Assets.Commands;

public class InitiateThumbnailUploadCommandHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly InitiateThumbnailUploadCommandHandler _sut;

    public InitiateThumbnailUploadCommandHandlerTests() =>
        _sut = new InitiateThumbnailUploadCommandHandler(_assetRepository, _storageService);

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new InitiateThumbnailUploadCommand(Guid.NewGuid(), "image/jpeg", 1024);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldReturnPresignedUrl()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var expiry = DateTime.UtcNow.AddMinutes(15);
        var command = new InitiateThumbnailUploadCommand(asset.Id, "image/jpeg", 1024);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);
        _storageService
            .GenerateThumbnailUploadUrlAsync(command.AssetId, command.ContentType, Arg.Any<CancellationToken>())
            .Returns(new PresignedUploadResult("https://s3.example.com/thumbnail", $"thumbnails/{asset.Id}/thumbnail", expiry));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.PresignedUrl.Should().Be("https://s3.example.com/thumbnail");
        result.ExpiresAt.Should().Be(expiry);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldNotWriteToDatabase()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var command = new InitiateThumbnailUploadCommand(asset.Id, "image/jpeg", 1024);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);
        _storageService
            .GenerateThumbnailUploadUrlAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PresignedUploadResult("https://s3.example.com/thumbnail", "thumbnails/x/thumbnail", DateTime.UtcNow.AddMinutes(15)));

        await _sut.Handle(command, CancellationToken.None);

        await _assetRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
