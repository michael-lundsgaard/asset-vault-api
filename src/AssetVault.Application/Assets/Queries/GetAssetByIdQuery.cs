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

            return new AssetResponse(
                asset.Id,
                asset.FileName,
                asset.ContentType,
                asset.Size.Bytes,
                asset.Size.ToString(),
                asset.Status.ToString(),
                asset.CreatedAt,
                Collection: request.Expand.HasFlag(AssetExpand.Collection) && asset.Collection is not null
                    ? new CollectionSummary(asset.Collection.Id, asset.Collection.Name)
                    : null,
                Tags: request.Expand.HasFlag(AssetExpand.Tags)
                    ? asset.Tags.Select(t => t.Name).ToList()
                    : null
            );
        }
    }
}