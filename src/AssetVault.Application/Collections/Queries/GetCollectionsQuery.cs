using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses;
using MediatR;

namespace AssetVault.Application.Collections.Queries
{
    public record GetCollectionsQuery(CollectionExpand Expand = CollectionExpand.None)
        : IRequest<IReadOnlyList<CollectionResponse>>;

    public class GetCollectionsQueryHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<GetCollectionsQuery, IReadOnlyList<CollectionResponse>>
    {
        public async Task<IReadOnlyList<CollectionResponse>> Handle(
            GetCollectionsQuery request,
            CancellationToken cancellationToken)
        {
            var collections = await collectionRepository.GetAllAsync(request.Expand, cancellationToken);
            return collections.Select(c => c.ToResponse(request.Expand)).ToList();
        }
    }
}
