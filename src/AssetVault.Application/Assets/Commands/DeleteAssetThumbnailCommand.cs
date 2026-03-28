using AssetVault.Application.Common.Interfaces;
using MediatR;

namespace AssetVault.Application.Assets.Commands
{
    public record DeleteAssetThumbnailCommand(Guid UserId, Guid AssetId) : IRequest;

    public class DeleteAssetThumbnailCommandHandler(
        IAssetRepository assetRepository,
        IStorageService storageService
    ) : IRequestHandler<DeleteAssetThumbnailCommand>
    {
        public async Task Handle(DeleteAssetThumbnailCommand request, CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            if (asset.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not have permission to modify this asset.");

            if (asset.ThumbnailUrl is null)
                return;

            await storageService.DeletePublicAsync($"thumbnails/{request.AssetId}/thumbnail", cancellationToken);
            asset.RemoveThumbnail();
            await assetRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
