using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Enums;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record AddAssetToCollectionCommand(Guid UserId, Guid CollectionId, Guid AssetId) : IRequest;

    public class AddAssetToCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IAssetRepository assetRepository
    ) : IRequestHandler<AddAssetToCollectionCommand>
    {
        public async Task Handle(AddAssetToCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.CollectionId} not found.");

            if (collection.Type != CollectionType.Shared && collection.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not have permission to modify this collection.");

            // Any authenticated user may add any asset to a Shared collection — asset ownership is not required.
            // AssetExpand.Collections is required so the domain method can de-duplicate
            var asset = await assetRepository.GetByIdAsync(request.AssetId, AssetExpand.Collections, cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            if (asset.Collections.Any(c => c.Id == request.CollectionId))
                return;

            asset.AddToCollection(collection);
            await assetRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
