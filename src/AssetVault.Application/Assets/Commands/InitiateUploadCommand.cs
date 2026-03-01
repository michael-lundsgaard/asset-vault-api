using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using MediatR;

namespace AssetVault.Application.Assets.Commands
{
    public record InitiateUploadCommand(
        Guid UserId,
        string FileName,
        string ContentType,
        long SizeBytes
    ) : IRequest<InitiateUploadResult>;

    public record InitiateUploadResult(
        Guid AssetId,
        string PresignedUrl,
        DateTime ExpiresAt
    );

    public class InitiateUploadCommandHandler(
        IAssetRepository assetRepository,
        IStorageService storageService
    ) : IRequestHandler<InitiateUploadCommand, InitiateUploadResult>
    {
        public async Task<InitiateUploadResult> Handle(
            InitiateUploadCommand request,
            CancellationToken cancellationToken)
        {
            var asset = MediaAsset.Create(
                request.UserId,
                request.FileName,
                request.ContentType,
                request.SizeBytes);

            var presigned = await storageService.GenerateUploadUrlAsync(
                asset.Id,
                request.FileName,
                request.ContentType,
                cancellationToken);

            asset.SetStoragePath(presigned.StoragePath);

            await assetRepository.AddAsync(asset, cancellationToken);
            await assetRepository.SaveChangesAsync(cancellationToken);

            return new InitiateUploadResult(asset.Id, presigned.PresignedUrl, presigned.ExpiresAt);
        }
    }

}
