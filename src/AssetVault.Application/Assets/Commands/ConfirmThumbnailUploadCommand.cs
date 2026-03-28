using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Assets;
using MediatR;

namespace AssetVault.Application.Assets.Commands
{
    public record ConfirmThumbnailUploadCommand(Guid AssetId) : IRequest<AssetResponse>;

    public class ConfirmThumbnailUploadCommandHandler(
        IAssetRepository assetRepository,
        IStorageService storageService
    ) : IRequestHandler<ConfirmThumbnailUploadCommand, AssetResponse>
    {
        public async Task<AssetResponse> Handle(
            ConfirmThumbnailUploadCommand request,
            CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            var thumbnailPath = $"thumbnails/{request.AssetId}/thumbnail";
            var thumbnailUrl = storageService.GetPublicUrl(thumbnailPath);
            asset.SetThumbnailUrl(thumbnailUrl);
            await assetRepository.SaveChangesAsync(cancellationToken);

            return asset.ToResponse(AssetExpand.None);
        }
    }
}
