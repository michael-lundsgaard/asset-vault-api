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
        List<string> Tags)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CollectionSummary>? Collections { get; init; } // omitted unless ?expand=collections
    }

    public record PresignedUploadResponse(
        Guid AssetId,
        string PresignedUrl,
        DateTime ExpiresAt
    );
}
