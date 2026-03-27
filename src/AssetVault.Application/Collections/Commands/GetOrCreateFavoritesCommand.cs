using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Domain.Entities;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record EnsureFavoritesCommand(Guid UserId) : IRequest<CollectionResponse>;

    public class EnsureFavoritesCommandHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<EnsureFavoritesCommand, CollectionResponse>
    {
        public async Task<CollectionResponse> Handle(
            EnsureFavoritesCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await collectionRepository.GetFavoritesAsync(request.UserId, cancellationToken);
            if (existing is not null)
                return existing.ToResponse(CollectionExpand.None);

            var favorites = Collection.CreateFavorites(request.UserId);
            await collectionRepository.AddAsync(favorites, cancellationToken);
            await collectionRepository.SaveChangesAsync(cancellationToken);

            return favorites.ToResponse(CollectionExpand.None);
        }
    }
}
