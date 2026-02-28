using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using MediatR;

namespace AssetVault.Application.Collections.Queries
{
    public record GetCollectionByIdQuery(Guid Id, CollectionExpand Expand = CollectionExpand.None)
        : IRequest<CollectionResponse?>;

    public class GetCollectionByIdQueryHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<GetCollectionByIdQuery, CollectionResponse?>
    {
        public async Task<CollectionResponse?> Handle(
            GetCollectionByIdQuery request,
            CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.Id, request.Expand, cancellationToken);

            if (collection is null) return null;

            return collection.ToResponse(request.Expand);
        }
    }
}
