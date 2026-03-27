using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Domain.Entities;
using AssetVault.Domain.Enums;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record CreateCollectionCommand(Guid UserId, string Name, string? Description, CollectionType Type = CollectionType.Shared)
        : IRequest<CollectionResponse>;

    public class CreateCollectionCommandHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<CreateCollectionCommand, CollectionResponse>
    {
        public async Task<CollectionResponse> Handle(
            CreateCollectionCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Type == CollectionType.Favorites)
                throw new InvalidOperationException("Favorites collections are created automatically. Use PUT /api/collections/my/favorites.");

            var collection = Collection.Create(request.UserId, request.Name, request.Description, request.Type);

            await collectionRepository.AddAsync(collection, cancellationToken);
            await collectionRepository.SaveChangesAsync(cancellationToken);

            return collection.ToResponse(CollectionExpand.None);
        }
    }
}
