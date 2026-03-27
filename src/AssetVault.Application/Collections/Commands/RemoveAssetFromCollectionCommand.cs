using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Enums;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record RemoveAssetFromCollectionCommand(Guid UserId, Guid CollectionId, Guid AssetId) : IRequest;

    public class RemoveAssetFromCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IAssetRepository assetRepository
    ) : IRequestHandler<RemoveAssetFromCollectionCommand>
    {
        public async Task Handle(RemoveAssetFromCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.CollectionId} not found.");

            if (collection.Type != CollectionType.Shared && collection.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not have permission to modify this collection.");

            // Any authenticated user may remove any asset from a Shared collection — asset ownership is not required.
            // AssetExpand.Collections is required so the domain method can locate and remove the entry
            var asset = await assetRepository.GetByIdAsync(request.AssetId, AssetExpand.Collections, cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            if (!asset.Collections.Any(c => c.Id == request.CollectionId))
                return;

            asset.RemoveFromCollection(collection);
            await assetRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
