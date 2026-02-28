using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Contracts.Responses.Common;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Collections.Queries
{
    public record GetCollectionsQuery(CollectionQuery Query) : IRequest<PaginatedResponse<CollectionResponse>>;

    public class GetCollectionsQueryValidator : AbstractValidator<GetCollectionsQuery>
    {
        public GetCollectionsQueryValidator()
        {
            RuleFor(x => x.Query).SetValidator(new CollectionQueryValidator());
        }
    }

    public class GetCollectionsQueryHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<GetCollectionsQuery, PaginatedResponse<CollectionResponse>>
    {
        public async Task<PaginatedResponse<CollectionResponse>> Handle(
            GetCollectionsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await collectionRepository.GetPagedAsync(request.Query, cancellationToken);
            return result.ToPaginatedResponse(request.Query.Expand);
        }
    }
}
