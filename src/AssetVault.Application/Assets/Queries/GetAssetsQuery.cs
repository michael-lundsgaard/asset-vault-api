using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses;
using MediatR;

namespace AssetVault.Application.Assets.Queries
{
    public record GetAssetsQuery(AssetExpand Expand = AssetExpand.None)
        : IRequest<List<AssetResponse>>;

    public class GetAssetsQueryHandler(IAssetRepository assetRepository)
        : IRequestHandler<GetAssetsQuery, List<AssetResponse>>
    {
        public async Task<List<AssetResponse>> Handle(
            GetAssetsQuery request,
            CancellationToken cancellationToken)
        {
            var assets = await assetRepository.GetAllAsync(
                request.Expand,
                cancellationToken);

            return [.. assets.Select(asset => asset.ToResponse(request.Expand))];
        }
    }
}