# Data Model: Asset Analytics Stats

**Phase**: 1 — Design  
**Date**: 2026-03-10

---

## Overview

This feature introduces **no new domain entities** and **no schema changes** to the `MediaAsset` table. All analytics data is derived from the existing `MediaAsset` entity via aggregate queries.

New types introduced are:

- Internal Application-layer result records (used by `IAnalyticsRepository` return values)
- Contract response DTOs in `AssetVault.Contracts`
- A `MimeTypeCategory` enum + `MimeCategoryResolver` helper in Application

---

## Existing Entity (read-only reference)

### `MediaAsset` — `AssetVault.Domain/Entities/MediaAsset.cs`

Relevant fields used by analytics queries:

| Field         | Type          | Notes                                             |
| ------------- | ------------- | ------------------------------------------------- |
| `Id`          | `Guid`        | Primary key                                       |
| `UserId`      | `Guid`        | FK → `UserProfile`; all analytics scoped to this  |
| `ContentType` | `string`      | MIME type string (e.g. `image/jpeg`, `video/mp4`) |
| `Size`        | `FileSize`    | Value Object; `Size.Bytes` (long) for sum         |
| `Status`      | `AssetStatus` | Only `Active` assets are counted                  |
| `CreatedAt`   | `DateTime`    | UTC; used for activity grouping by calendar day   |

**No new fields are added. No schema migration is needed for columns.**

A new composite database index is added via EF Core configuration (see research.md §4).

---

## New Internal Result Records — Application Layer

These records are used as the return types of `IAnalyticsRepository` methods. They live in the Application layer and are **not** exposed directly to the API — they are mapped to contract response types.

### `AnalyticsSummary`

```csharp
// Application/Analytics/
public record AnalyticsSummary(int TotalAssets, long TotalStorageBytes);
```

| Field               | Type   | Description                                    |
| ------------------- | ------ | ---------------------------------------------- |
| `TotalAssets`       | `int`  | Count of Active assets owned by the user       |
| `TotalStorageBytes` | `long` | Sum of `Size.Bytes` for all Active user assets |

### `DailyUploadCount`

```csharp
public record DailyUploadCount(DateOnly Date, int Count);
```

| Field   | Type       | Description                                  |
| ------- | ---------- | -------------------------------------------- |
| `Date`  | `DateOnly` | Calendar day (UTC)                           |
| `Count` | `int`      | Number of Active assets uploaded on this day |

### `MimeTypeCategoryCount`

```csharp
public record MimeTypeCategoryCount(MimeTypeCategory Category, int Count);
```

| Field      | Type               | Description                              |
| ---------- | ------------------ | ---------------------------------------- |
| `Category` | `MimeTypeCategory` | Enum value for the MIME type group       |
| `Count`    | `int`              | Number of Active assets in this category |

---

## New Enum — Application Layer

### `MimeTypeCategory`

```csharp
// Application/Analytics/
public enum MimeTypeCategory
{
    Image,
    Video,
    Audio,
    Document,
    Other
}
```

---

## New Helper — Application Layer

### `MimeCategoryResolver`

```csharp
// Application/Analytics/
public static class MimeCategoryResolver
{
    public static MimeTypeCategory Categorise(string contentType) => contentType switch
    {
        _ when contentType.StartsWith("image/")  => MimeTypeCategory.Image,
        _ when contentType.StartsWith("video/")  => MimeTypeCategory.Video,
        _ when contentType.StartsWith("audio/")  => MimeTypeCategory.Audio,
        _ when IsDocument(contentType)           => MimeTypeCategory.Document,
        _                                        => MimeTypeCategory.Other
    };

    private static bool IsDocument(string ct) =>
        ct == "application/pdf"
        || ct.StartsWith("text/")
        || ct.StartsWith("application/msword")
        || ct.StartsWith("application/vnd.openxmlformats-officedocument.wordprocessingml")
        || ct.StartsWith("application/vnd.ms-");
}
```

---

## New Interface — Application Layer

### `IAnalyticsRepository`

```csharp
// Application/Common/Interfaces/IAnalyticsRepository.cs
public interface IAnalyticsRepository
{
    Task<AnalyticsSummary> GetSummaryAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyUploadCount>> GetUploadActivityAsync(
        Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MimeTypeCategoryCount>> GetCategoryBreakdownAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
```

---

## New Contract Response DTOs — `AssetVault.Contracts`

Located in `AssetVault.Contracts/Responses/Analytics/AnalyticsResponses.cs`.

### `AnalyticsSummaryResponse`

```csharp
public record AnalyticsSummaryResponse(int TotalAssets, long TotalStorageBytes);
```

### `DailyActivityEntry`

```csharp
public record DailyActivityEntry(DateOnly Date, int Count);
```

### `UploadActivityResponse`

```csharp
public record UploadActivityResponse(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<DailyActivityEntry> Activity);
```

### `AssetCategoryEntry`

```csharp
public record AssetCategoryEntry(string Category, int Count);
```

### `AssetCategoriesResponse`

```csharp
public record AssetCategoriesResponse(IReadOnlyList<AssetCategoryEntry> Categories);
```

---

## New EF Core Index Configuration

Added to `MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>` in Infrastructure:

```csharp
builder.HasIndex(a => new { a.UserId, a.Status, a.CreatedAt })
       .HasDatabaseName("IX_Assets_UserId_Status_CreatedAt");
```

This enables efficient WHERE + GROUP BY queries for all three analytics endpoints.

**Migration name**: `AddAnalyticsIndex`

---

## Validation Rules

### `GetUploadActivityQuery`

| Field  | Rule                        | Error (FR ref) |
| ------ | --------------------------- | -------------- |
| `From` | Must be ≤ `To`              | FR-008, 400    |
| `To`   | Must be ≤ `From + 365 days` | FR-007, 400    |

Both fields are optional (`DateOnly?`); when absent, the handler defaults:

- `from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-29))`
- `to = DateOnly.FromDateTime(DateTime.UtcNow)`

Validation only applies when values are provided.

---

## State Transitions

None — this feature is entirely read-only. No `MediaAsset` state transitions occur.
