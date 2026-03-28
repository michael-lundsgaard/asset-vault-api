using AssetVault.Domain.Common;
using AssetVault.Domain.Events;

namespace AssetVault.Domain.Entities
{
    public class Collection : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public ICollection<MediaAsset> Assets { get; private set; } = [];
        public string? CoverImageUrl { get; private set; }

        // Navigation
        public UserProfile Owner { get; private set; } = default!;

        private Collection() { }

        public static Collection Create(Guid userId, string name, string? description = null) =>
            new() { UserId = userId, Name = name, Description = description };

        public void Update(string name, string? description)
        {
            Name = name;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCoverImageUrl(string url)
        {
            CoverImageUrl = url;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CollectionCoverSetEvent(Id));
        }

        public void RemoveCoverImage()
        {
            CoverImageUrl = null;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CollectionCoverRemovedEvent(Id));
        }
    }
}
