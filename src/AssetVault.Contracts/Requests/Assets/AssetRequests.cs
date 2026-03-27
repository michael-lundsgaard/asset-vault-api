namespace AssetVault.Contracts.Requests.Assets
{
    public record InitiateUploadRequest(
        string FileName,
        string ContentType,
        long SizeBytes
    );

    public record RenameAssetRequest(string FileName);

    public record GetAssetsRequest
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Search { get; init; }
        public string? ContentType { get; init; }
        public List<string>? Tags { get; init; }
        public string? Status { get; init; }
        public string? SortBy { get; init; }
        public bool SortDescending { get; init; } = false;
    }
}
