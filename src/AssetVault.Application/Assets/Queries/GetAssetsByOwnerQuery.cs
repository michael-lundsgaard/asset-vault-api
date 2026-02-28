using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Assets;
using AssetVault.Contracts.Responses.Common;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Assets.Queries
{
    public record GetAssetsByOwnerQuery(Guid UserId, AssetQuery Query) : IRequest<PaginatedResponse<AssetResponse>>;

    public class GetAssetsByOwnerQueryValidator : AbstractValidator<GetAssetsByOwnerQuery>
    {
        public GetAssetsByOwnerQueryValidator()
        {
            RuleFor(x => x.Query).SetValidator(new AssetQueryValidator());
        }
    }

    public class GetAssetsByOwnerQueryHandler(IAssetRepository assetRepository)
        : IRequestHandler<GetAssetsByOwnerQuery, PaginatedResponse<AssetResponse>>
    {
        public async Task<PaginatedResponse<AssetResponse>> Handle(
            GetAssetsByOwnerQuery request,
            CancellationToken cancellationToken)
        {
            var result = await assetRepository.GetPagedByUserAsync(request.UserId, request.Query, cancellationToken);
            return result.ToPaginatedResponse(request.Query.Expand);
        }
    }
}
