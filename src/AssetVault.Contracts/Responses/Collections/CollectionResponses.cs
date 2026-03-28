using System.Text.Json.Serialization;
using AssetVault.Contracts.Responses.Assets;

namespace AssetVault.Contracts.Responses.Collections
{

    public record CollectionSummary(Guid Id, string Name);

    public record CollectionResponse(
        Guid Id,
        Guid UserId,
        string Name,
        string? Description,
        DateTime CreatedAt)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CoverImageUrl { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AssetResponse>? Assets { get; init; }
    }

}
