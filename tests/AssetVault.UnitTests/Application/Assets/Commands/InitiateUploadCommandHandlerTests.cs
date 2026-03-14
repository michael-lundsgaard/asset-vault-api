using AssetVault.Application.Assets.Commands;
using AssetVault.Application.Common.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace AssetVault.UnitTests.Application.Assets.Commands;

public class InitiateUploadCommandHandlerTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly InitiateUploadCommandHandler _sut;

    public InitiateUploadCommandHandlerTests() =>
        _sut = new InitiateUploadCommandHandler(_assetRepository, _storageService);

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldCallStorageServiceWithCorrectArgs()
    {
        var command = new InitiateUploadCommand(Guid.NewGuid(), "photo.jpg", "image/jpeg", 1024);
        _storageService
            .GenerateUploadUrlAsync(Arg.Any<Guid>(), command.FileName, command.ContentType, Arg.Any<CancellationToken>())
            .Returns(new PresignedUploadResult("https://s3.example.com/upload", "uploads/id/photo.jpg", DateTime.UtcNow.AddMinutes(15)));

        await _sut.Handle(command, CancellationToken.None);

        await _storageService.Received(1).GenerateUploadUrlAsync(
            Arg.Any<Guid>(),
            command.FileName,
            command.ContentType,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldAddAssetAndSaveChanges()
    {
        var command = new InitiateUploadCommand(Guid.NewGuid(), "photo.jpg", "image/jpeg", 1024);
        _storageService
            .GenerateUploadUrlAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PresignedUploadResult("https://s3.example.com/upload", "uploads/id/photo.jpg", DateTime.UtcNow.AddMinutes(15)));

        await _sut.Handle(command, CancellationToken.None);

        await _assetRepository.Received(1).AddAsync(Arg.Any<MediaAsset>(), Arg.Any<CancellationToken>());
        await _assetRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldReturnResultWithPresignedUrl()
    {
        var command = new InitiateUploadCommand(Guid.NewGuid(), "photo.jpg", "image/jpeg", 1024);
        var expiry = DateTime.UtcNow.AddMinutes(15);
        _storageService
            .GenerateUploadUrlAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PresignedUploadResult("https://s3.example.com/upload", "uploads/id/photo.jpg", expiry));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.PresignedUrl.Should().Be("https://s3.example.com/upload");
        result.ExpiresAt.Should().Be(expiry);
        result.AssetId.Should().NotBeEmpty();
    }
}
