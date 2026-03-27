using AssetVault.Application.Assets.Commands;
using AssetVault.Contracts.Responses.Assets;

namespace AssetVault.UnitTests.Application.Assets.Commands;

public class RenameAssetCommandHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly RenameAssetCommandHandler _sut;

    public RenameAssetCommandHandlerTests() =>
        _sut = new RenameAssetCommandHandler(_assetRepository);

    [Fact]
    public async Task Handle_GivenValidRequest_ShouldRenameAssetAndReturnResponse()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "original.mp4", "video/mp4", 1024);
        var command = new RenameAssetCommand(asset.Id, "renamed.mp4");
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeOfType<AssetResponse>();
        result.FileName.Should().Be("renamed.mp4");
        await _assetRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new RenameAssetCommand(Guid.NewGuid(), "renamed.mp4");
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
