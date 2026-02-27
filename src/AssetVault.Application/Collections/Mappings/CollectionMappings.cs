using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses;
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
                collection.CreatedAt)
            {
                Assets = expand.HasFlag(CollectionExpand.Assets)
                    ? [.. collection.Assets.Select(a => a.ToResponse(AssetExpand.None))]
                    : null
            };
    }
}
