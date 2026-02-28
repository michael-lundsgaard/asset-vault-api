using AssetVault.Application.Common.Interfaces;

namespace AssetVault.Application.Collections.Queries
{
    public enum CollectionSortBy
    {
        CreatedAt,
        Name,
    }

    public record CollectionQuery(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        CollectionSortBy SortBy = CollectionSortBy.CreatedAt,
        bool SortDescending = false,
        CollectionExpand Expand = CollectionExpand.None);
}
