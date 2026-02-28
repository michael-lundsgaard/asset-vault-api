namespace AssetVault.API.Extensions
{
    /// <summary>
    /// Parses the ?expand= query string into any flags enum.
    /// Usage: GET /api/assets/{id}?expand=collections
    /// Usage: GET /api/collections/{id}?expand=assets
    /// </summary>
    public static class ExpandParser
    {
        public static T Parse<T>(string? expand) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(expand)) return default;

            var result = default(T);
            var parts = expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                if (Enum.TryParse<T>(part, ignoreCase: true, out var flag))
                    result = CombineFlags(result, flag);
            }

            return result;
        }

        private static T CombineFlags<T>(T a, T b) where T : struct, Enum
        {
            var intA = Convert.ToInt32(a);
            var intB = Convert.ToInt32(b);
            return (T)Enum.ToObject(typeof(T), intA | intB);
        }
    }
}
