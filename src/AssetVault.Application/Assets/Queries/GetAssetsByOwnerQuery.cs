using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses;
using MediatR;

namespace AssetVault.Application.Assets.Queries
{
    public record GetAssetsByOwnerQuery(Guid OwnerId, AssetExpand Expand = AssetExpand.None)
        : IRequest<List<AssetResponse>>;

    public class GetAssetsByOwnerQueryHandler(IAssetRepository assetRepository)
        : IRequestHandler<GetAssetsByOwnerQuery, List<AssetResponse>>
    {
        public async Task<List<AssetResponse>> Handle(
            GetAssetsByOwnerQuery request,
            CancellationToken cancellationToken)
        {
            var assets = await assetRepository.GetByOwnerAsync(
                request.OwnerId,
                request.Expand,
                cancellationToken);

            // Map domain entities to contract DTOs
            return [.. assets.Select(asset => asset.ToResponse(request.Expand))];
        }
    }
}