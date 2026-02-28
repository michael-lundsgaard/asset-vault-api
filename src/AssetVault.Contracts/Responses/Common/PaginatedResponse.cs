namespace AssetVault.Contracts.Responses.Common
{
    public class PaginatedResponse<T>
    {
        public required IReadOnlyList<T> Items { get; set; }
        public required int Total { get; set; }
        public required int Page { get; set; }
        public required int PageSize { get; set; }
        public bool HasNextPage => Page * PageSize < Total;
        public bool HasPreviousPage => Page > 1;
    }
}