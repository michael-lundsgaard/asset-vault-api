using AssetVault.Application.Assets.Commands;
using AssetVault.Domain.Enums;

namespace AssetVault.UnitTests.Application.Assets.Commands;

public class ConfirmUploadCommandHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly ConfirmUploadCommandHandler _sut;

    public ConfirmUploadCommandHandlerTests() =>
        _sut = new ConfirmUploadCommandHandler(_assetRepository);

    [Fact]
    public async Task Handle_GivenAssetNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new ConfirmUploadCommand(Guid.NewGuid(), Guid.NewGuid());
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_GivenWrongOwner_ShouldThrowUnauthorizedAccessException()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "file.jpg", "image/jpeg", 512);
        var command = new ConfirmUploadCommand(Guid.NewGuid(), asset.Id); // different UserId
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldSetAssetStatusToActive()
    {
        var userId = Guid.NewGuid();
        var asset = MediaAsset.Create(userId, "file.jpg", "image/jpeg", 512);
        var command = new ConfirmUploadCommand(userId, asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        asset.Status.Should().Be(AssetStatus.Active);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldSaveChanges()
    {
        var userId = Guid.NewGuid();
        var asset = MediaAsset.Create(userId, "file.jpg", "image/jpeg", 512);
        var command = new ConfirmUploadCommand(userId, asset.Id);
        _assetRepository.GetByIdAsync(command.AssetId, cancellationToken: Arg.Any<CancellationToken>()).Returns(asset);

        await _sut.Handle(command, CancellationToken.None);

        await _assetRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
