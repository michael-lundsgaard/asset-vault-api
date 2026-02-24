using AssetVault.Domain.Common;
using AssetVault.Domain.Enums;
using AssetVault.Domain.Events;
using AssetVault.Domain.ValueObjects;

namespace AssetVault.Domain.Entities
{
    public class MediaAsset : BaseEntity
    {
        public string FileName { get; private set; } = default!;
        public string ContentType { get; private set; } = default!;
        public FileSize Size { get; private set; } = default!;
        public StoragePath StoragePath { get; private set; } = default!;
        public AssetStatus Status { get; private set; }
        public Guid? CollectionId { get; private set; }

        // Navigation
        public Collection? Collection { get; private set; }
        public ICollection<Tag> Tags { get; private set; } = [];

        private MediaAsset() { } // Required for EF Core and to enforce use of the static Create method

        public static MediaAsset Create(
            string fileName,
            string contentType,
            long sizeInBytes,
            string storagePath,
            Guid? collectionId = null)
        {
            var asset = new MediaAsset
            {
                FileName = fileName,
                ContentType = contentType,
                Size = FileSize.Create(sizeInBytes),
                StoragePath = StoragePath.Create(storagePath),
                Status = AssetStatus.Pending,
                CollectionId = collectionId
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

        public void AddTag(Tag tag)
        {
            if (!Tags.Any(t => t.Name == tag.Name))
                Tags.Add(tag);
        }

        public void SetStoragePath(string path)
        {
            StoragePath = StoragePath.Create(path);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}