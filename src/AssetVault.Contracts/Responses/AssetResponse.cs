namespace AssetVault.Contracts.Responses
{
    public record AssetResponse(
        Guid Id,
        string FileName,
        string ContentType,
        long SizeBytes,
        string SizeFormatted,
        string Status,
        DateTime CreatedAt,
        CollectionSummary? Collection = null,   // null unless ?expand=collection
        List<string>? Tags = null               // null unless ?expand=tags
    );

    public record CollectionSummary(Guid Id, string Name);

    public record CollectionResponse(
        Guid Id,
        string Name,
        string? Description,
        DateTime CreatedAt,
        List<AssetResponse>? Assets = null      // null unless ?expand=assets
    );

    public record PresignedUploadResponse(
        Guid AssetId,
        string UploadUrl,
        DateTime UrlExpiresAt
    );
}