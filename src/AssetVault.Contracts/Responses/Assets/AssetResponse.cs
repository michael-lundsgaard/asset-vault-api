using System.Text.Json.Serialization;
using AssetVault.Contracts.Responses.Collections;

namespace AssetVault.Contracts.Responses.Assets
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
        List<string> Tags,
        string? ThumbnailUrl)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CollectionSummary>? Collections { get; init; }
    }

    public record PresignedUploadResponse(
        Guid AssetId,
        string PresignedUrl,
        DateTime ExpiresAt
    );

    public record PresignedDownloadResponse(
        Guid AssetId,
        string PresignedUrl,
        DateTime ExpiresAt
    );
}
