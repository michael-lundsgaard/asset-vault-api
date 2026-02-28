using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Contracts.Responses.Common;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Collections.Queries
{
    public record GetCollectionsByOwnerQuery(Guid UserId, CollectionQuery Query) : IRequest<PaginatedResponse<CollectionResponse>>;

    public class GetCollectionsByOwnerQueryValidator : AbstractValidator<GetCollectionsByOwnerQuery>
    {
        public GetCollectionsByOwnerQueryValidator()
        {
            RuleFor(x => x.Query).SetValidator(new CollectionQueryValidator());
        }
    }

    public class GetCollectionsByOwnerQueryHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<GetCollectionsByOwnerQuery, PaginatedResponse<CollectionResponse>>
    {
        public async Task<PaginatedResponse<CollectionResponse>> Handle(
            GetCollectionsByOwnerQuery request,
            CancellationToken cancellationToken)
        {
            var result = await collectionRepository.GetPagedByUserAsync(request.UserId, request.Query, cancellationToken);
            return result.ToPaginatedResponse(request.Query.Expand);
        }
    }
}
