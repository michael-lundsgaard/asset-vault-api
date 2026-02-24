using AssetVault.Domain.Common;

namespace AssetVault.Domain.Entities
{
    public class Collection : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public ICollection<MediaAsset> Assets { get; private set; } = [];

        private Collection() { } // Required for EF Core and to enforce use of the static Create method

        public static Collection Create(string name, string? description = null) =>
            new() { Name = name, Description = description };
    }
}