using System.Text.Json.Serialization;

namespace AssetVault.Contracts.Responses
{
    public record AssetResponse(
        Guid Id,
        Guid UserId,
        string FileName,
        string ContentType,
        long SizeBytes,
        string SizeFormatted,
        string Status,
        DateTime CreatedAt,
        List<string> Tags)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CollectionSummary>? Collections { get; init; } // omitted unless ?expand=collections
    }

    public record CollectionSummary(Guid Id, string Name);

    public record CollectionResponse(
        Guid Id,
        Guid UserId,
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