using System.Text.Json.Serialization;
using AssetVault.Contracts.Responses.Assets;

namespace AssetVault.Contracts.Responses.Collections
{

    public record CollectionSummary(Guid Id, string Name, string? CoverImageUrl);

    public record CollectionResponse(
        Guid Id,
        Guid UserId,
        string Name,
        string? Description,
        DateTime CreatedAt,
        string? CoverImageUrl)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AssetResponse>? Assets { get; init; }
    }

}
