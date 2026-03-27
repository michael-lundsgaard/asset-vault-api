namespace AssetVault.Contracts.Requests.Collections
{
    public record CreateCollectionRequest(
        string Name,
        string? Description = null,
        string Type = "Shared"
    );

    public record UpdateCollectionRequest(
        string Name,
        string? Description = null
    );

    public record GetCollectionsRequest
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Search { get; init; }
        public string? SortBy { get; init; }
        public bool SortDescending { get; init; } = false;
    }
}
