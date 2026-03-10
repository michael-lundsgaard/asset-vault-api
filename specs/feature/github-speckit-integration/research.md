# Research: Asset Analytics Stats

**Phase**: 0 — Resolve unknowns before design  
**Date**: 2026-03-10

---

## 1. EF Core Aggregate Query Strategy

**Decision**: Use EF Core 9 async aggregate operators (`SumAsync`, `CountAsync`) directly on `IQueryable<MediaAsset>` scoped to `UserId` and `Status = Active`.

**Rationale**: EF Core translates `SumAsync` and `CountAsync` into efficient single `SELECT SUM / COUNT` SQL statements entirely on the database server. For the summary query (total count + total storage), two separate aggregate calls (`CountAsync` + `SumAsync`) are issued — this avoids materialising any rows. For the categories breakdown, `GroupBy(a => MimeCategoryOf(a.ContentType))` cannot execute client-side grouping logic on a computed value in Npgsql; instead, the `ContentType` column is grouped client-side after fetching only `(ContentType, COUNT)` projections via `GroupBy(a => a.ContentType).Select(g => new { g.Key, Count = g.Count() })` — safe because the result set is small (distinct MIME types per user, typically < 50 rows).

**Alternatives Considered**:

- _Single SQL view / raw SQL_: Too much Infrastructure coupling; would require raw string SQL or a database migration for a view — overly complex for this read-only feature.
- _Summarising in-memory from full asset list_: Would materialise thousands of rows. Rejected.

---

## 2. Date Gap-Fill Strategy

**Decision**: Execute the EF Core `GROUP BY DATE_TRUNC('day', CreatedAt)` query to retrieve only days that have uploads, then merge with a fully populated date range generated in the application layer.

**Rationale**: Generating a complete list of dates (from `from` to `to` inclusive) in .NET is trivial with `Enumerable.Range` and `DateTime.AddDays`. Filling gaps in the DB via `generate_series` requires raw Npgsql SQL. The EF query returns at most 365 rows (capped by FR-007); left-joining them in memory with the pre-generated date list is O(n) where n ≤ 365 — well within acceptable performance.

**Implementation sketch**:

```csharp
// 1. Query DB: days-with-uploads in range
var uploadsByDay = await context.Assets
    .Where(a => a.UserId == userId && a.Status == AssetStatus.Active
                && a.CreatedAt >= fromUtc && a.CreatedAt < toUtcExclusive)
    .GroupBy(a => a.CreatedAt.Date)
    .Select(g => new { Date = g.Key, Count = g.Count() })
    .ToDictionaryAsync(x => x.Date, x => x.Count, ct);

// 2. Fill gaps in app layer
var result = Enumerable.Range(0, (int)(toDate - fromDate).TotalDays + 1)
    .Select(d => fromDate.AddDays(d))
    .Select(date => new DailyActivityEntry(date, uploadsByDay.GetValueOrDefault(date, 0)))
    .ToList();
```

Note: `a.CreatedAt.Date` translates to `DATE_TRUNC('day', "CreatedAt")` in Npgsql EF Core 9 when `CreatedAt` is `timestamp with time zone` stored in UTC.

**Alternatives Considered**:

- _PostgreSQL `generate_series`_: Requires raw SQL or a DB function — harder to test, leaks DB logic into Infrastructure. Rejected.
- _Client-side full materialisation_: Loads all asset rows just to group in memory. Rejected.

---

## 3. MIME Type Categorisation

**Decision**: Define a static helper in the Application layer (`MimeTypeCategory` enum + `MimeCategoryResolver.Categorise(string contentType)` static method) that maps a MIME type string to a category. The Infrastructure analytics repository uses this helper when building the categories breakdown.

**Category mapping rules** (from spec Assumptions):

| Prefix / Pattern                                                                                                                              | Category |
| --------------------------------------------------------------------------------------------------------------------------------------------- | -------- |
| `image/*`                                                                                                                                     | Image    |
| `video/*`                                                                                                                                     | Video    |
| `audio/*`                                                                                                                                     | Audio    |
| `application/pdf`, `text/*`, `application/msword`, `application/vnd.openxmlformats-officedocument.wordprocessingml.*`, `application/vnd.ms-*` | Document |
| everything else                                                                                                                               | Other    |

**Rationale**: The categorisation logic is a pure function with no I/O — it belongs in the Application layer, not Infrastructure. Placing it in Application keeps it unit-testable without any DB dependency. The infrastructure repository calls `MimeCategoryResolver.Categorise` when constructing the response after retrieving MIME type aggregates from the DB.

**DB query approach**: Group by the raw ContentType string in the DB (`GROUP BY ContentType`), return `(ContentType, Count)` pairs, then categorise and sum in memory. This correctly handles the "always return all 5 categories" requirement: after aggregation, ensure all 5 `MimeTypeCategory` enum values are present with a default count of 0.

**Alternatives Considered**:

- _Categorise in SQL via CASE expression_: Would duplicate the mapping logic in raw SQL and in tests. Rejected.
- _Enum stored on MediaAsset_: A schema change; the spec explicitly states no schema changes are required. Rejected.

---

## 4. Database Indexing Recommendations

**Decision**: Add a composite index on `(UserId, Status, CreatedAt)` on the `Assets` table via an EF Core `HasIndex` configuration. This covers all three analytics queries:

| Query      | Index usage                                                                             |
| ---------- | --------------------------------------------------------------------------------------- |
| Summary    | `WHERE UserId = ? AND Status = 1` → index on (UserId, Status)                           |
| Activity   | `WHERE UserId = ? AND Status = 1 AND CreatedAt >= ? AND CreatedAt < ?` → full composite |
| Categories | `WHERE UserId = ? AND Status = 1` → index on (UserId, Status)                           |

**Rationale**: The existing `MediaAsset` EF configuration likely has a primary key index on `Id` only. An additional index on `(UserId, Status, CreatedAt)` will allow all analytics queries to be answered with an index range scan rather than a full table scan, meeting the < 1 second target at 10,000+ assets.

**Migration**: Add `HasIndex(a => new { a.UserId, a.Status, a.CreatedAt }).HasDatabaseName("IX_Assets_UserId_Status_CreatedAt")` to `MediaAssetConfiguration`. Run `dotnet ef migrations add AddAnalyticsIndex`.

**Alternatives Considered**:

- _Separate indexes per column_: Less effective for multi-column WHERE clauses. Rejected.
- _No index change_: Table scan would be acceptable at < 10k rows but will degrade. Rejected per SC-001.

---

## 5. Analytics Repository Interface Design

**Decision**: Introduce a dedicated `IAnalyticsRepository` interface in `Application/Common/Interfaces/` with three methods:

```csharp
public interface IAnalyticsRepository
{
    Task<AnalyticsSummary> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyUploadCount>> GetUploadActivityAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MimeTypeCategoryCount>> GetCategoryBreakdownAsync(Guid userId, CancellationToken cancellationToken = default);
}
```

Internal result types (`AnalyticsSummary`, `DailyUploadCount`, `MimeTypeCategoryCount`) live as simple records in the Application layer. Contract response types (`AnalyticsSummaryResponse`, etc.) are in Contracts and are mapped from the internal results.

**Rationale**: A dedicated interface keeps analytics concerns separate from `IAssetRepository` (which manages the asset CRUD lifecycle). Following the existing pattern keeps handlers injectable and mockable with NSubstitute.

**Alternatives Considered**:

- _Adding methods to IAssetRepository_: Would bloat a repository that already has clear CRUD responsibilities. Rejected.
- _Handlers calling IAssetRepository directly for aggregate projections_: Would require `IAssetRepository` to expose `IQueryable`, violating encapsulation. Rejected.

---

## 6. Validation Rules (FluentValidation)

**Decision**: `GetUploadActivityQuery` requires a validator; the other two queries have no parameters to validate.

`GetUploadActivityQueryValidator` rules:

- `from <= to` (FR-008) → `Must((q, to) => q.From <= to)`
- `(to - from).TotalDays <= 365` (FR-007) → `Must(...)`
- Both `from` and `to` are optional; when absent, handler defaults to last 30 days — no validation needed for absence

`GetAnalyticsSummaryQuery` and `GetAssetCategoriesQuery` take only `Guid userId` — no additional rules needed beyond pipeline presence.

---

## Summary of Decisions

| #   | Topic               | Decision                                                                        |
| --- | ------------------- | ------------------------------------------------------------------------------- |
| 1   | Aggregates          | EF Core `SumAsync`/`CountAsync` + MIME type grouped SELECT projection           |
| 2   | Date gap-fill       | DB query returns upload days only; app layer fills gaps with `Enumerable.Range` |
| 3   | MIME categorisation | Static `MimeCategoryResolver` helper in Application layer                       |
| 4   | Indexes             | Composite `(UserId, Status, CreatedAt)` via EF Core `HasIndex` + new migration  |
| 5   | Repository          | New `IAnalyticsRepository` interface; internal result records in Application    |
| 6   | Validation          | FluentValidation on activity query only; other queries have no parameters       |
