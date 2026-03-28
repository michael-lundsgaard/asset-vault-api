using AssetVault.Domain.Common;

namespace AssetVault.Domain.Events
{
    public record CollectionCoverSetEvent(Guid CollectionId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    public record CollectionCoverRemovedEvent(Guid CollectionId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
