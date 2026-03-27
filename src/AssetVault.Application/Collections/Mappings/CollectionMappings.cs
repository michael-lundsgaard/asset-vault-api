using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Contracts.Responses.Common;
using AssetVault.Domain.Entities;

namespace AssetVault.Application.Collections.Mappings
{
    public static class CollectionMappings
    {
        public static CollectionResponse ToResponse(this Collection collection, CollectionExpand expand) =>
            new(
                collection.Id,
                collection.UserId,
                collection.Name,
                collection.Description,
                collection.Type.ToString(),
                collection.CreatedAt)
            {
                Assets = expand.HasFlag(CollectionExpand.Assets)
                    ? [.. collection.Assets.Select(a => a.ToResponse(AssetExpand.None))]
                    : null
            };

        public static PaginatedResponse<CollectionResponse> ToPaginatedResponse(
            this PagedResult<Collection> result,
            CollectionExpand expand) =>
            new()
            {
                Items = result.Items.Select(c => c.ToResponse(expand)).ToList(),
                Total = result.Total,
                Page = result.PageNumber,
                PageSize = result.PageSize
            };
    }
}
