using AssetVault.Domain.Common;
using AssetVault.Domain.Enums;
using AssetVault.Domain.Events;
using AssetVault.Domain.ValueObjects;

namespace AssetVault.Domain.Entities
{
    public class MediaAsset : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string FileName { get; private set; } = default!;
        public string ContentType { get; private set; } = default!;
        public FileSize Size { get; private set; } = default!;
        public StoragePath? StoragePath { get; private set; }
        public AssetStatus Status { get; private set; }
        public List<string> Tags { get; private set; } = [];

        // Navigation
        public UserProfile Owner { get; private set; } = default!;
        public ICollection<Collection> Collections { get; private set; } = [];

        private MediaAsset() { }

        public static MediaAsset Create(
            Guid userId,
            string fileName,
            string contentType,
            long sizeInBytes)
        {
            var asset = new MediaAsset
            {
                UserId = userId,
                FileName = fileName,
                ContentType = contentType,
                Size = FileSize.Create(sizeInBytes),
                Status = AssetStatus.Pending,
            };

            asset.AddDomainEvent(new AssetCreatedEvent(asset.Id, fileName));
            return asset;
        }

        public void MarkAsUploaded()
        {
            Status = AssetStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AssetUploadedEvent(Id));
        }

        public void MarkAsFailed()
        {
            Status = AssetStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddTag(string tag)
        {
            var normalized = tag.ToLowerInvariant();
            if (!Tags.Contains(normalized))
                Tags.Add(normalized);
        }

        public void RemoveTag(string tag) => Tags.Remove(tag.ToLowerInvariant());

        public void AddToCollection(Collection collection)
        {
            if (!Collections.Any(c => c.Id == collection.Id))
                Collections.Add(collection);
        }

        public void RemoveFromCollection(Collection collection)
        {
            var existing = Collections.FirstOrDefault(c => c.Id == collection.Id);
            if (existing is not null) Collections.Remove(existing);
        }

        public void SetStoragePath(string path)
        {
            StoragePath = StoragePath.Create(path);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}