using AssetVault.Application.Common.Interfaces;

namespace AssetVault.API.Extensions
{
    /// <summary>
    /// Parses the ?expand= query string into expand flags enums.
    /// Usage: GET /api/assets/{id}?expand=collections
    /// Usage: GET /api/collections/{id}?expand=assets
    /// </summary>
    public static class ExpandParser
    {
        public static AssetExpand Parse(string? expand)
        {
            if (string.IsNullOrWhiteSpace(expand)) return AssetExpand.None;

            var result = AssetExpand.None;
            var parts = expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                if (Enum.TryParse<AssetExpand>(part, ignoreCase: true, out var flag))
                    result |= flag;
            }

            return result;
        }

        public static CollectionExpand ParseCollectionExpand(string? expand)
        {
            if (string.IsNullOrWhiteSpace(expand)) return CollectionExpand.None;

            var result = CollectionExpand.None;
            var parts = expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                if (Enum.TryParse<CollectionExpand>(part, ignoreCase: true, out var flag))
                    result |= flag;
            }

            return result;
        }
    }
}
