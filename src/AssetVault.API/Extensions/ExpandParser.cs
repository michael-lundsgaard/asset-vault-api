using AssetVault.Application.Common.Interfaces;

namespace AssetVault.API.Extensions
{
    /// <summary>
    /// Parses the ?expand= query string into the AssetExpand flags enum.
    /// Usage: GET /api/assets/{id}?expand=collection,tags
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
    }
}