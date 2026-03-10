# Implementation Plan: Asset Analytics Stats

**Branch**: `001-asset-analytics-stats` | **Date**: 2026-03-10 | **Spec**: [specs/001-asset-analytics-stats/spec.md](../../001-asset-analytics-stats/spec.md)
**Input**: Feature specification from `/specs/001-asset-analytics-stats/spec.md`

## Summary

Add three read-only analytics endpoints under `GET /api/analytics` for the authenticated user: a summary of total Active asset count and total storage bytes; upload activity grouped by calendar day for a configurable date range (default 30 days, max 365); and asset counts broken down by MIME type category (image, video, audio, document, other). No new domain entities are introduced — all queries read from the existing `MediaAsset` entity. Implementation follows Clean Architecture: new `IAnalyticsRepository` interface in Application, EF Core implementation in Infrastructure, three MediatR query handlers with FluentValidation, manual response mappings, and a thin `AnalyticsController` in the API.

## Technical Context

**Language/Version**: .NET 9 / ASP.NET Core 9  
**Primary Dependencies**: MediatR 12, FluentValidation 11, EF Core 9 + Npgsql  
**Storage**: PostgreSQL via Supabase; EF Core aggregate queries on the existing `MediaAsset` table  
**Testing**: xUnit + FluentAssertions + NSubstitute (unit); Testcontainers + WebApplicationFactory (integration)  
**Target Platform**: Linux server (containerised ASP.NET Core API)  
**Project Type**: web-service  
**Performance Goals**: < 1 second for all analytics queries against libraries up to 10,000 assets (SC-001); < 100 ms for validation errors (SC-003)  
**Constraints**: Analytics scoped strictly to the authenticated user (FR-010); Pending assets excluded from all counts; date range capped at 365 days (FR-007); UTC-only calendar-day grouping  
**Scale/Scope**: Per-user aggregate queries; single-tenant performance target of 10,000 assets; no caching needed at this scale

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

- [x] **I. Clean Architecture**: `IAnalyticsRepository` defined in `Application/Common/Interfaces/`; EF Core implementation in Infrastructure; controller in API layer only. Dependency flow is strictly API → Application → Domain. ✅
- [x] **II. CQRS via MediatR**: Three read-only MediatR queries (`GetAnalyticsSummaryQuery`, `GetUploadActivityQuery`, `GetAssetCategoriesQuery`), each with a FluentValidation validator. Handlers injected with `IAnalyticsRepository` only. ✅
- [x] **III. Test Quality Gate**: Unit tests planned for all three handlers using NSubstitute mocks; integration test planned for `GET /api/analytics/*` endpoints via Testcontainers. ✅
- [x] **IV. Presigned URL Storage Pattern**: Not applicable — this feature is read-only analytics; no file storage operations. ✅ (N/A)
- [x] **V. Domain Integrity**: No domain entity state mutations. `MediaAsset` entity is read from, not written to. No domain events are raised. ✅ (N/A for read-only)

## Project Structure

### Documentation (this feature)

```text
specs/feature/github-speckit-integration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── AssetVault.Domain/
│   └── (no changes — feature reads from existing MediaAsset entity)
├── AssetVault.Application/
│   └── Analytics/
│       ├── Queries/
│       │   ├── GetAnalyticsSummaryQuery.cs     # Query + Handler + Validator
│       │   ├── GetUploadActivityQuery.cs       # Query + Handler + Validator
│       │   └── GetAssetCategoriesQuery.cs      # Query + Handler + Validator
│       └── Mappings/
│           └── AnalyticsMappings.cs            # ToResponse(...) extension methods
│   └── Common/
│       └── Interfaces/
│           └── IAnalyticsRepository.cs         # New interface
├── AssetVault.Contracts/
│   └── Responses/
│       └── Analytics/
│           └── AnalyticsResponses.cs           # AnalyticsSummaryResponse, DailyActivityEntry, AssetCategoryBreakdownResponse
├── AssetVault.Infrastructure/
│   └── Persistence/
│       ├── Repositories/
│       │   └── AnalyticsRepository.cs          # EF Core aggregate query implementation
│       └── Extensions/
│           └── InfrastructureServiceExtensions.cs  # Register IAnalyticsRepository
└── AssetVault.API/
    └── Controllers/
        └── AnalyticsController.cs              # GET /api/analytics/summary, /activity, /categories

tests/
├── AssetVault.UnitTests/
│   └── Analytics/
│       ├── GetAnalyticsSummaryQueryHandlerTests.cs
│       ├── GetUploadActivityQueryHandlerTests.cs
│       └── GetAssetCategoriesQueryHandlerTests.cs
└── AssetVault.IntegrationTests/
    └── Analytics/
        └── AnalyticsEndpointsTests.cs
```

**Structure Decision**: No new `AssetVault.Domain` files are required — the feature is purely read-only and introduces no new entities, value objects, or domain events. The new `IAnalyticsRepository` interface is defined in `Application/Common/Interfaces/` (consistent with `IAssetRepository`). All aggregate database queries live in the Infrastructure implementation, keeping EF Core details out of the Application layer.

## Complexity Tracking

No Constitution Check violations. All gates pass without exception or justification required.
