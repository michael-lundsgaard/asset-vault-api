using AssetVault.Application.Common.Interfaces;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record RemoveAssetFromCollectionCommand(Guid CollectionId, Guid AssetId) : IRequest;

    public class RemoveAssetFromCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IAssetRepository assetRepository
    ) : IRequestHandler<RemoveAssetFromCollectionCommand>
    {
        public async Task Handle(RemoveAssetFromCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.CollectionId} not found.");

            // AssetExpand.Collections is required so the domain method can locate and remove the entry
            var asset = await assetRepository.GetByIdAsync(request.AssetId, AssetExpand.Collections, cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            if (!asset.Collections.Any(c => c.Id == request.CollectionId))
            {
                // Asset is not in the collection, so we can skip
                return;
            }

            asset.RemoveFromCollection(collection);
            await assetRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
