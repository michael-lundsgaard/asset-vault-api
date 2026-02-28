using AssetVault.Application.Common;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Assets;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Contracts.Responses.Common;
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

        public static PaginatedResponse<AssetResponse> ToPaginatedResponse(
            this PagedResult<MediaAsset> result,
            AssetExpand expand) =>
            new()
            {
                Items = result.Items.Select(a => a.ToResponse(expand)).ToList(),
                Total = result.Total,
                Page = result.PageNumber,
                PageSize = result.PageSize
            };
    }
}
