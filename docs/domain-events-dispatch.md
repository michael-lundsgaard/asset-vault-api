# Domain Events

Domain events represent something meaningful that happened in the domain. They decouple the code that causes a change from the code that reacts to it — the handler that confirms an upload doesn't need to know about thumbnails, audit logs, or notifications.

## Current events

| Event                | Raised in                     | When                               |
| -------------------- | ----------------------------- | ---------------------------------- |
| `AssetCreatedEvent`  | `MediaAsset.Create()`         | Upload initiated (status: Pending) |
| `AssetUploadedEvent` | `MediaAsset.MarkAsUploaded()` | Upload confirmed (status: Active)  |

Events accumulate in `BaseEntity._domainEvents` but are not yet dispatched. Dispatch needs to be wired into `AppDbContext`.

## How to wire up dispatch

1. Inject `IPublisher` (MediatR) into `AppDbContext`
2. Override `SaveChangesAsync` to collect, save, then publish:

```csharp
// AppDbContext.cs
private readonly IPublisher _publisher;

public AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher) : base(options)
{
    _publisher = publisher;
}

public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    // Collect before saving — entities may be detached after SaveChanges
    var events = ChangeTracker.Entries<BaseEntity>()
        .SelectMany(e => e.Entity.DomainEvents)
        .ToList();

    var result = await base.SaveChangesAsync(ct);

    foreach (var domainEvent in events)
        await _publisher.Publish(domainEvent, ct);

    ChangeTracker.Entries<BaseEntity>()
        .ToList()
        .ForEach(e => e.Entity.ClearDomainEvents());

    return result;
}
```

3. Implement `INotificationHandler<TEvent>` for each event you want to handle.

## Planned handlers

| Event                        | Handler                    | Action                                                     |
| ---------------------------- | -------------------------- | ---------------------------------------------------------- |
| `AssetCreatedEvent`          | `LogAssetCreatedHandler`   | Write audit log entry                                      |
| `AssetUploadedEvent`         | `GenerateThumbnailHandler` | Enqueue background job for thumbnail / video transcode     |
| `AssetUploadedEvent`         | `NotifyFollowersHandler`   | Push notification to followers (when social features land) |
| `AssetDeletedEvent` (future) | `DeleteFromStorageHandler` | Remove file from R2 asynchronously                         |

## Example handler

```csharp
// Application/Assets/EventHandlers/GenerateThumbnailHandler.cs
public class GenerateThumbnailHandler(IBackgroundJobService jobs)
    : INotificationHandler<AssetUploadedEvent>
{
    public Task Handle(AssetUploadedEvent notification, CancellationToken cancellationToken)
    {
        jobs.Enqueue(() => GenerateThumbnailJob.Run(notification.AssetId));
        return Task.CompletedTask;
    }
}
```
