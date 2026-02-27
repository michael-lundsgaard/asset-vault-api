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
                asset.UserId,
                asset.FileName,
                asset.ContentType,
                asset.Size.Bytes,
                asset.Size.ToString(),
                asset.Status.ToString(),
                asset.CreatedAt,
                asset.Tags)
            {
                Collections = expand.HasFlag(AssetExpand.Collections)
                    ? [.. asset.Collections.Select(c => new CollectionSummary(c.Id, c.Name))]
                    : null
            };
    }
}
