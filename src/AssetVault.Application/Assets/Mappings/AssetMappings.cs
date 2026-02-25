using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses;
using AssetVault.Domain.Entities;

namespace AssetVault.Application.Assets.Mappings
{
    public static class AssetMappings
    {
        public static AssetResponse ToResponse(this MediaAsset asset, AssetExpand expand) =>
            new(
                asset.Id,
                asset.FileName,
                asset.ContentType,
                asset.Size.Bytes,
                asset.Size.ToString(),
                asset.Status.ToString(),
                asset.CreatedAt)
            {
                Collection = expand.HasFlag(AssetExpand.Collection) && asset.Collection is not null
                    ? new CollectionSummary(asset.Collection.Id, asset.Collection.Name)
                    : null,
                Tags = expand.HasFlag(AssetExpand.Tags)
                    ? [.. asset.Tags.Select(t => t.Name)]
                    : null
            };
    }
}
