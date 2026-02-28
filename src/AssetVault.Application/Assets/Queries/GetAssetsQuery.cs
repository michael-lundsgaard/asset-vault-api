using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Assets;
using AssetVault.Contracts.Responses.Common;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Assets.Queries
{
    public record GetAssetsQuery(AssetQuery Query) : IRequest<PaginatedResponse<AssetResponse>>;

    public class GetAssetsQueryValidator : AbstractValidator<GetAssetsQuery>
    {
        public GetAssetsQueryValidator()
        {
            RuleFor(x => x.Query.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Query.PageSize).InclusiveBetween(1, 100);
        }
    }

    public class GetAssetsQueryHandler(IAssetRepository assetRepository)
        : IRequestHandler<GetAssetsQuery, PaginatedResponse<AssetResponse>>
    {
        public async Task<PaginatedResponse<AssetResponse>> Handle(
            GetAssetsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await assetRepository.GetPagedAsync(request.Query, cancellationToken);
            return result.ToPaginatedResponse(request.Query.Expand);
        }
    }
}