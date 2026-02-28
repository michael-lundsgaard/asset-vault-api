using AssetVault.Domain.Common;

namespace AssetVault.Domain.Events
{
    public record AssetCreatedEvent(Guid AssetId, string FileName) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    public record AssetUploadedEvent(Guid AssetId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
