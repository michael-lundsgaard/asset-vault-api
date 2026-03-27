using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Contracts.Responses.Common;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Collections.Queries
{
    public record GetCollectionsByUserQuery(Guid UserId, CollectionQuery Query) : IRequest<PaginatedResponse<CollectionResponse>>;

    public class GetCollectionsByUserQueryValidator : AbstractValidator<GetCollectionsByUserQuery>
    {
        public GetCollectionsByUserQueryValidator()
        {
            RuleFor(x => x.Query).SetValidator(new CollectionQueryValidator());
        }
    }

    public class GetCollectionsByUserQueryHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<GetCollectionsByUserQuery, PaginatedResponse<CollectionResponse>>
    {
        public async Task<PaginatedResponse<CollectionResponse>> Handle(
            GetCollectionsByUserQuery request,
            CancellationToken cancellationToken)
        {
            var result = await collectionRepository.GetPagedByUserAsync(request.UserId, request.Query, cancellationToken);
            return result.ToPaginatedResponse(request.Query.Expand);
        }
    }
}
