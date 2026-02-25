using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses;
using MediatR;

namespace AssetVault.Application.Assets.Queries
{
    public record GetAssetByIdQuery(Guid Id, AssetExpand Expand = AssetExpand.None)
        : IRequest<AssetResponse?>;

    public class GetAssetByIdQueryHandler(IAssetRepository assetRepository)
        : IRequestHandler<GetAssetByIdQuery, AssetResponse?>
    {
        public async Task<AssetResponse?> Handle(
            GetAssetByIdQuery request,
            CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdWithExpandAsync(
                request.Id,
                request.Expand,
                cancellationToken);

            if (asset is null) return null;

            return asset.ToResponse(request.Expand);
        }
    }
}