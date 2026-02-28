namespace AssetVault.Contracts.Responses.Common
{
    public record PaginatedResponse<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public required int Total { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public bool HasNextPage => Page * PageSize < Total;
        public bool HasPreviousPage => Page > 1;
    }
}

