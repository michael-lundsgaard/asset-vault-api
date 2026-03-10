# Tasks: Asset Analytics Stats

**Input**: Design documents from `/specs/001-asset-analytics-stats/` + `/specs/feature/github-speckit-integration/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/analytics-endpoints.md ✅, quickstart.md ✅

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Exact file paths are included in all descriptions

## Path Conventions

- **Application**: `src/AssetVault.Application/Analytics/`
- **Contracts**: `src/AssetVault.Contracts/Responses/Analytics/`
- **Infrastructure**: `src/AssetVault.Infrastructure/Persistence/`
- **API**: `src/AssetVault.API/Controllers/`
- **Unit tests**: `tests/AssetVault.UnitTests/Analytics/`
- **Integration tests**: `tests/AssetVault.IntegrationTests/Analytics/`

---

## Phase 1: Setup (Test Project Infrastructure)

**Purpose**: Create the test projects that all analytics unit and integration tests depend on.

- [ ] T001 Create `tests/AssetVault.UnitTests/AssetVault.UnitTests.csproj` targeting .NET 9 with xUnit, FluentAssertions, NSubstitute, and a project reference to `AssetVault.Application`
- [ ] T002 [P] Create `tests/AssetVault.IntegrationTests/AssetVault.IntegrationTests.csproj` targeting .NET 9 with xUnit, FluentAssertions, Testcontainers.PostgreSql, and project references to `AssetVault.API` and `AssetVault.Infrastructure`
- [ ] T003 Add `tests/AssetVault.UnitTests/AssetVault.UnitTests.csproj` and `tests/AssetVault.IntegrationTests/AssetVault.IntegrationTests.csproj` to `AssetVault.slnx`

**Checkpoint**: `dotnet build` succeeds for both test projects.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared types, interface, repository implementation, DI registration, and database index that ALL three user stories depend on. No story work can begin until this phase is complete.

**⚠️ CRITICAL**: Each user story's handler, mapping, and controller task depends on the interface and repository being in place.

- [ ] T004 [P] Create contract response DTOs (`AnalyticsSummaryResponse`, `DailyActivityEntry`, `UploadActivityResponse`, `AssetCategoryEntry`, `AssetCategoriesResponse`) in `src/AssetVault.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- [ ] T005 [P] Create internal Application result records (`AnalyticsSummary`, `DailyUploadCount`, `MimeTypeCategoryCount`) in `src/AssetVault.Application/Analytics/AnalyticsResults.cs`
- [ ] T006 [P] Create `MimeTypeCategory` enum (`Image`, `Video`, `Audio`, `Document`, `Other`) in `src/AssetVault.Application/Analytics/MimeTypeCategory.cs`
- [ ] T007 Create `MimeCategoryResolver` static helper with `Categorise(string contentType)` method in `src/AssetVault.Application/Analytics/MimeCategoryResolver.cs` (depends on T006)
- [ ] T008 Create `IAnalyticsRepository` interface with `GetSummaryAsync`, `GetUploadActivityAsync`, and `GetCategoryBreakdownAsync` in `src/AssetVault.Application/Common/Interfaces/IAnalyticsRepository.cs` (depends on T005)
- [ ] T009 Implement `AnalyticsRepository` with EF Core aggregate queries for all three methods in `src/AssetVault.Infrastructure/Persistence/Repositories/AnalyticsRepository.cs` — use gap-fill strategy from research.md §2 for activity, client-side MIME grouping strategy from research.md §3 for categories (depends on T007, T008)
- [ ] T010 Register `IAnalyticsRepository` → `AnalyticsRepository` as scoped in `src/AssetVault.Infrastructure/Extensions/InfrastructureServiceExtensions.cs` (depends on T009)
- [ ] T011 Add composite index `HasIndex(a => new { a.UserId, a.Status, a.CreatedAt }).HasDatabaseName("IX_Assets_UserId_Status_CreatedAt")` to `src/AssetVault.Infrastructure/Persistence/Configurations/MediaAssetConfiguration.cs` and run `dotnet ef migrations add AddAnalyticsIndex --project src/AssetVault.Infrastructure --startup-project src/AssetVault.API`

**Checkpoint**: `dotnet build` succeeds; migration file created. User story implementation can now begin.

---

## Phase 3: User Story 1 — Asset Summary Overview (Priority: P1) 🎯 MVP

**Goal**: Authenticated users can retrieve their total Active asset count and total storage bytes consumed via `GET /api/analytics/summary`.

**Independent Test**: Call `GET /api/analytics/summary` as an authenticated user; verify `totalAssets` and `totalStorageBytes` reflect only that user's Active assets. Unauthenticated calls return `401`.

### Tests for User Story 1

- [ ] T012 [P] [US1] Write unit tests covering all 5 acceptance scenarios for `GetAnalyticsSummaryQueryHandler` in `tests/AssetVault.UnitTests/Analytics/GetAnalyticsSummaryQueryHandlerTests.cs` using NSubstitute mock for `IAnalyticsRepository`

### Implementation for User Story 1

- [ ] T013 [US1] Create `GetAnalyticsSummaryQuery(Guid UserId)` record, `GetAnalyticsSummaryQueryHandler`, and no-op validator in `src/AssetVault.Application/Analytics/Queries/GetAnalyticsSummaryQuery.cs` — handler calls `IAnalyticsRepository.GetSummaryAsync` and returns `AnalyticsSummaryResponse` (depends on T008, T004)
- [ ] T014 [US1] Create `AnalyticsMappings.cs` with `ToSummaryResponse(this AnalyticsSummary summary)` extension method in `src/AssetVault.Application/Analytics/Mappings/AnalyticsMappings.cs` (depends on T005, T004)
- [ ] T015 [US1] Create `AnalyticsController` with `[Authorize]` attribute and `GET /api/analytics/summary` endpoint that sends `GetAnalyticsSummaryQuery` and returns `Ok(result)` in `src/AssetVault.API/Controllers/AnalyticsController.cs` (depends on T013)

**Checkpoint**: `GET /api/analytics/summary` returns accurate totals for the authenticated user. `401` for unauthenticated requests. User Story 1 is independently testable and deployable as MVP.

---

## Phase 4: User Story 2 — Upload Activity Over Time (Priority: P2)

**Goal**: Authenticated users can retrieve their daily upload counts for a configurable date range (default 30 days, max 365) via `GET /api/analytics/activity`.

**Independent Test**: Call `GET /api/analytics/activity` with and without date range parameters; verify per-day upload counts match known upload history. Days with no uploads return `count: 0`. Invalid date ranges return `400`.

### Tests for User Story 2

- [ ] T016 [P] [US2] Write unit tests covering all 5 acceptance scenarios and validator error cases for `GetUploadActivityQueryHandler` in `tests/AssetVault.UnitTests/Analytics/GetUploadActivityQueryHandlerTests.cs` using NSubstitute mock for `IAnalyticsRepository`

### Implementation for User Story 2

- [ ] T017 [US2] Create `GetUploadActivityQuery(Guid UserId, DateOnly? From, DateOnly? To)` record, `GetUploadActivityQueryValidator` (FluentValidation: `From <= To`, range ≤ 365 days), and `GetUploadActivityQueryHandler` in `src/AssetVault.Application/Analytics/Queries/GetUploadActivityQuery.cs` — handler defaults `From` to `DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))` and `To` to today when null (depends on T008, T004)
- [ ] T018 [US2] Add `ToActivityResponse(this IReadOnlyList<DailyUploadCount> activity, DateOnly from, DateOnly to)` extension method to `src/AssetVault.Application/Analytics/Mappings/AnalyticsMappings.cs` (depends on T005, T004)
- [ ] T019 [US2] Add `GET /api/analytics/activity` endpoint accepting optional `from` and `to` query parameters to `src/AssetVault.API/Controllers/AnalyticsController.cs` — parse `DateOnly?` from query string, send `GetUploadActivityQuery`, return `Ok(result)` or `BadRequest` on `ValidationException` (depends on T017)

**Checkpoint**: `GET /api/analytics/activity` returns accurate day-by-day counts including zero-fill gaps. Date range validation returns `400` with descriptive errors. User Stories 1 AND 2 both work independently.

---

## Phase 5: User Story 3 — Asset Breakdown by MIME Type Category (Priority: P3)

**Goal**: Authenticated users can retrieve their Active asset counts broken down by MIME type category (image, video, audio, document, other) via `GET /api/analytics/categories`. All 5 categories always present, even with count 0.

**Independent Test**: Call `GET /api/analytics/categories`; verify that returned counts match the known distribution of MIME types across the user's Active assets. Response always contains all 5 category entries.

### Tests for User Story 3

- [ ] T020 [P] [US3] Write unit tests covering all 4 acceptance scenarios for `GetAssetCategoriesQueryHandler` in `tests/AssetVault.UnitTests/Analytics/GetAssetCategoriesQueryHandlerTests.cs` using NSubstitute mock for `IAnalyticsRepository`

### Implementation for User Story 3

- [ ] T021 [US3] Create `GetAssetCategoriesQuery(Guid UserId)` record, no-op validator, and `GetAssetCategoriesQueryHandler` in `src/AssetVault.Application/Analytics/Queries/GetAssetCategoriesQuery.cs` — handler calls `IAnalyticsRepository.GetCategoryBreakdownAsync` and returns `AssetCategoriesResponse` with all 5 categories always populated (depends on T008, T004, T006)
- [ ] T022 [US3] Add `ToCategoriesResponse(this IReadOnlyList<MimeTypeCategoryCount> counts)` extension method to `src/AssetVault.Application/Analytics/Mappings/AnalyticsMappings.cs` — ensures all 5 `MimeTypeCategory` enum values are always present with default 0 (depends on T005, T006, T004)
- [ ] T023 [US3] Add `GET /api/analytics/categories` endpoint to `src/AssetVault.API/Controllers/AnalyticsController.cs` (depends on T021)

**Checkpoint**: All three user stories are independently functional. All unit tests pass.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Integration tests covering the full HTTP stack and final validation.

- [ ] T024 [P] Write integration tests for `GET /api/analytics/summary`, `GET /api/analytics/activity`, and `GET /api/analytics/categories` covering auth, data isolation, and default parameters in `tests/AssetVault.IntegrationTests/Analytics/AnalyticsEndpointsTests.cs` using `WebApplicationFactory<Program>` + Testcontainers PostgreSQL
- [ ] T025 Run `dotnet test` and verify all acceptance scenarios from quickstart.md pass end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately. T001, T002 can run in parallel.
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS all user stories. T004, T005, T006 can start in parallel; T007 depends on T006; T008 depends on T005; T009 depends on T007 + T008; T010 depends on T009; T011 is independent.
- **Phase 3 (US1)**: Depends on Phase 2 completion. T012 parallelizable with T013.
- **Phase 4 (US2)**: Depends on Phase 2 completion. T016 parallelizable with T017.
- **Phase 5 (US3)**: Depends on Phase 2 completion. T020 parallelizable with T021.
- **Phase 6 (Polish)**: Depends on Phases 3, 4, 5.

### User Story Dependencies

- **User Story 1 (P1)**: Foundational complete → independently testable ✅
- **User Story 2 (P2)**: Foundational complete + US1 controller scaffold available for endpoint addition
- **User Story 3 (P3)**: Foundational complete + US1/US2 controller scaffold available for endpoint addition

### Within Each User Story

- Tests (T012, T016, T020) can be written before or in parallel with handler implementation
- Handler before mapping before controller endpoint
- Unit test must exercise handler directly via `IAnalyticsRepository` mock

---

## Parallel Examples

### Phase 2 (Foundational) Parallel Start

```
Parallel:  T004 (contracts DTOs)
           T005 (result records)
           T006 (enum)
           T011 (EF index + migration)
Then:      T007 (resolver, needs T006)
Then:      T008 (interface, needs T005)
Then:      T009 (repository, needs T007 + T008)
Then:      T010 (DI registration, needs T009)
```

### Once Foundational Complete — User Stories in Parallel (if multi-developer)

```
Developer A: T012 → T013 → T014 → T015   (User Story 1)
Developer B: T016 → T017 → T018 → T019   (User Story 2)
Developer C: T020 → T021 → T022 → T023   (User Story 3)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: `GET /api/analytics/summary` works end-to-end with a real database
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready
2. Phase 3 (US1) → Summary endpoint live → MVP!
3. Phase 4 (US2) → Activity endpoint live
4. Phase 5 (US3) → Categories endpoint live
5. Phase 6 → Full test coverage + validated

---

## Task Summary

| Phase              | Tasks        | User Story | Parallelizable         |
| ------------------ | ------------ | ---------- | ---------------------- |
| 1 — Setup          | T001–T003    | —          | T001, T002             |
| 2 — Foundational   | T004–T011    | —          | T004, T005, T006, T011 |
| 3 — US1 Summary    | T012–T015    | US1 (P1)   | T012                   |
| 4 — US2 Activity   | T016–T019    | US2 (P2)   | T016                   |
| 5 — US3 Categories | T020–T023    | US3 (P3)   | T020                   |
| 6 — Polish         | T024–T025    | —          | T024                   |
| **Total**          | **25 tasks** |            |                        |

**MVP scope**: Phases 1–3 (15 tasks) → `GET /api/analytics/summary` live and tested.
