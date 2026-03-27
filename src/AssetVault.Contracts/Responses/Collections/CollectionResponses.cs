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
        string Type,
        DateTime CreatedAt)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AssetResponse>? Assets { get; init; } // omitted unless ?expand=assets
    }

}
