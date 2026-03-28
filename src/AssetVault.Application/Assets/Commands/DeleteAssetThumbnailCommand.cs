using AssetVault.Application.Common.Interfaces;
using MediatR;

namespace AssetVault.Application.Assets.Commands
{
    public record DeleteAssetThumbnailCommand(Guid AssetId) : IRequest;

    public class DeleteAssetThumbnailCommandHandler(
        IAssetRepository assetRepository,
        IStorageService storageService
    ) : IRequestHandler<DeleteAssetThumbnailCommand>
    {
        public async Task Handle(DeleteAssetThumbnailCommand request, CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            if (asset.ThumbnailUrl is null)
                return;

            await storageService.DeletePublicAsync($"thumbnails/{request.AssetId}/thumbnail", cancellationToken);
            asset.RemoveThumbnail();
            await assetRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
