using System.Text.Json.Serialization;

namespace AssetVault.Contracts.Responses
{
    public record AssetResponse(
        Guid Id,
        string FileName,
        string ContentType,
        long SizeBytes,
        string SizeFormatted,
        string Status,
        DateTime CreatedAt)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CollectionSummary? Collection { get; init; } // omitted unless ?expand=collection

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Tags { get; init; } // omitted unless ?expand=tags
    }

    public record CollectionSummary(Guid Id, string Name);

    public record CollectionResponse(
        Guid Id,
        string Name,
        string? Description,
        DateTime CreatedAt)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AssetResponse>? Assets { get; init; } // omitted unless ?expand=assets
    }

    public record PresignedUploadResponse(
        Guid AssetId,
        string UploadUrl,
        DateTime UrlExpiresAt
    );
}