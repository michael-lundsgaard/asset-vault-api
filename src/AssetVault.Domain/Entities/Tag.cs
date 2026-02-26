using AssetVault.Domain.Common;

namespace AssetVault.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public ICollection<MediaAsset> Assets { get; private set; } = [];

        // Required for EF Core and to enforce use of the static Create method
        private Tag() { }

        public static Tag Create(string name) =>
            new() { Name = name.ToLowerInvariant() };
    }
}