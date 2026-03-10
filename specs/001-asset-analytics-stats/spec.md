# Feature Specification: Asset Analytics Stats

**Feature Branch**: `001-asset-analytics-stats`  
**Created**: 2026-03-10  
**Status**: Draft  
**Input**: User description: "Add analytics endpoints that return asset statistics for the authenticated user: total asset count, total storage used in bytes, upload activity grouped by day for a configurable date range, and asset count broken down by MIME type category. All endpoints must follow Clean Architecture — MediatR query handlers, FluentValidation, EF Core queries against the existing Asset entity. Expose via GET /api/analytics."

## Clarifications

### Session 2026-03-10

- Q: Which timestamp field on `MediaAsset` should the upload activity query group by — `CreatedAt`, `UpdatedAt`, or a new `UploadedAt` field? → A: `CreatedAt` (immutable upload-initiation timestamp; no schema change required)
- Q: Should days with zero uploads be included as explicit entries in the activity response, or omitted? → A: Include all days in the requested range with count 0 for days with no uploads — consistent shape for client-side chart rendering
- Q: Should MIME type categories with zero assets be included in the categories response, or omitted? → A: Always return all 5 categories (image, video, audio, document, other) — fixed shape eliminates client-side missing-key handling

## User Scenarios & Testing _(mandatory)_

### User Story 1 — Asset Summary Overview (Priority: P1)

A dashboard user wants to see a high-level snapshot of their media library: how many assets they own and how much total storage they are consuming. This is the most fundamental piece of analytics — it gives users immediate awareness of their library size and storage footprint without requiring any filtering.

**Why this priority**: This is the foundational analytics capability. Every other metric builds on knowing how many confirmed assets a user has and how much space they consume.

**Independent Test**: Can be fully tested by calling `GET /api/analytics/summary` as an authenticated user and verifying the response contains an accurate total asset count and total storage bytes reflecting only that user's confirmed (Active) assets.

**Acceptance Scenarios**:

1. **Given** an authenticated user with 12 Active assets totalling 500 MB, **When** they call `GET /api/analytics/summary`, **Then** the response contains `totalAssets: 12` and `totalStorageBytes: 524288000`
2. **Given** an authenticated user with zero assets, **When** they call `GET /api/analytics/summary`, **Then** the response contains `totalAssets: 0` and `totalStorageBytes: 0`
3. **Given** two users each owning separate assets, **When** each calls `GET /api/analytics/summary`, **Then** each receives counts reflecting only their own assets
4. **Given** a user with both Pending and Active assets, **When** they call `GET /api/analytics/summary`, **Then** only Active assets are counted
5. **Given** an unauthenticated caller, **When** calling `GET /api/analytics/summary`, **Then** the response is `401 Unauthorized`

---

### User Story 2 — Upload Activity Over Time (Priority: P2)

A user wants to visualize how frequently they have been uploading assets over a period of time, grouped by calendar day. This helps them understand their own usage patterns and identify periods of high or low activity.

**Why this priority**: This is a key engagement metric for dashboards. The configurable date range makes it flexible enough to power both summary widgets (last 7 days) and detailed reports (last year). It is the most complex query but provides high value.

**Independent Test**: Can be fully tested by calling `GET /api/analytics/activity` with and without date range parameters and verifying the day-by-day upload counts match known upload history.

**Acceptance Scenarios**:

1. **Given** an authenticated user who uploaded 3 assets on 2026-02-01 and 1 asset on 2026-02-03 (by `CreatedAt` in UTC), **When** they call `GET /api/analytics/activity?from=2026-02-01&to=2026-02-05`, **Then** the response contains entries for 2026-02-01 with count 3 and 2026-02-03 with count 1
2. **Given** no `from`/`to` parameters supplied, **When** the user calls `GET /api/analytics/activity`, **Then** the response covers the last 30 calendar days by default
3. **Given** a date range where the user has no uploads, **When** the user calls `GET /api/analytics/activity`, **Then** the response contains one entry per calendar day in the requested range, each with `count: 0`
4. **Given** a `from` date later than `to`, **When** the user calls `GET /api/analytics/activity`, **Then** the response is `400 Bad Request` with a descriptive validation error
5. **Given** a date range exceeding the maximum allowed window, **When** the user calls `GET /api/analytics/activity`, **Then** the response is `400 Bad Request` indicating the allowed maximum

---

### User Story 3 — Asset Breakdown by MIME Type Category (Priority: P3)

A user wants to understand the composition of their media library — how many assets belong to each broad content category (images, videos, audio files, documents, and other). This helps them understand whether their storage is dominated by a particular type of media.

**Why this priority**: This gives actionable composition insight but does not block other analytics. It is lower priority because categories can be derived post-launch from existing summary data if needed.

**Independent Test**: Can be fully tested by calling `GET /api/analytics/categories` and verifying that the returned category counts match the known distribution of MIME types across the user's Active assets.

**Acceptance Scenarios**:

1. **Given** a user with 10 images, 5 videos, and 2 PDFs, **When** they call `GET /api/analytics/categories`, **Then** the response contains `image: 10`, `video: 5`, `document: 2`
2. **Given** a user with assets of an unrecognised MIME type, **When** they call `GET /api/analytics/categories`, **Then** those assets appear under an `other` category
3. **Given** a user with no assets in a given category, **When** they call `GET /api/analytics/categories`, **Then** the response still includes that category with `count: 0` — all 5 categories (image, video, audio, document, other) are always present
4. **Given** an unauthenticated caller, **When** calling `GET /api/analytics/categories`, **Then** the response is `401 Unauthorized`

---

### Edge Cases

- What happens when a user is newly registered with no assets? → All summary counts return 0, activity returns an empty list, categories returns empty/all-zero result
- How does the system handle assets that are in `Pending` status (upload initiated but not confirmed)? → Excluded from all analytics counts
- What if both `from` and `to` are the same date? → The range is treated as a single day and returns that day's upload count
- What if `from` or `to` is in the future? → Future dates are accepted; they will simply yield zero-count entries since no uploads have occurred there
- What is the maximum allowed date range for upload activity? → Assumed 365 days (see Assumptions)

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: The system MUST expose analytics endpoints under `GET /api/analytics`
- **FR-002**: The system MUST return the total count of **Active** assets owned by the authenticated user via `GET /api/analytics/summary`
- **FR-003**: The system MUST return the total storage consumed (sum of file sizes in bytes) of the authenticated user's Active assets via `GET /api/analytics/summary`
- **FR-004**: The system MUST return upload counts grouped by calendar day for the authenticated user's Active assets via `GET /api/analytics/activity`
- **FR-005**: The system MUST accept optional `from` and `to` date query parameters (inclusive, ISO 8601 date format) to configure the activity date range
- **FR-006**: When no date range is provided, the system MUST default to the last 30 calendar days for activity data
- **FR-007**: The system MUST enforce a maximum date range of 365 days on activity queries and return a validation error when exceeded
- **FR-008**: The system MUST validate that `from` is not later than `to` and return a validation error if it is
- **FR-009**: The system MUST return asset counts broken down by MIME type category (image, video, audio, document, other) via `GET /api/analytics/categories`
- **FR-010**: Analytics data MUST be scoped strictly to the authenticated user's own assets — no cross-user data leakage
- **FR-011**: All analytics endpoints MUST require authentication and return `401 Unauthorized` for unauthenticated requests

### Key Entities

- **Analytics Summary**: Aggregate totals for an authenticated user — total Active asset count and total storage bytes
- **Daily Activity Entry**: A pairing of a calendar date and the count of assets uploaded (confirmed) on that day
- **MIME Type Category Group**: A pairing of a broad content category name and the count of Active assets belonging to that category
- **MIME Type Category**: An enumerated classification (image, video, audio, document, other) derived from the MIME type prefix of an asset's file type

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: An authenticated user can retrieve their full asset analytics in under 1 second for libraries of up to 10,000 assets
- **SC-002**: All three analytics endpoints return accurate, user-scoped data with zero cross-user data leakage
- **SC-003**: Date range validation prevents invalid or oversized queries before any database work is performed, returning a descriptive error within 100 ms
- **SC-004**: A user with no assets receives valid zero-value responses from all endpoints rather than errors
- **SC-005**: The upload activity endpoint correctly reflects day-boundary grouping in the user's query timezone (UTC assumed by default)

## Assumptions

- Assets are tracked in UTC; calendar-day grouping for upload activity uses `MediaAsset.CreatedAt` truncated to UTC midnight boundaries (immutable upload-initiation timestamp — no schema change required)
- The maximum configurable date range for upload activity is **365 days**
- Active status means the upload has been confirmed via the existing confirm-upload flow; Pending assets are excluded from all analytics
- MIME type categories are derived from the MIME type prefix: `image/*` → image, `video/*` → video, `audio/*` → audio, `application/pdf` + `text/*` + `application/msword` family → document, everything else → other
- The feature introduces no new domain entities; it reads from the existing `MediaAsset` entity
- Performance targets assume standard database indexing on `UserId`, `Status`, `CreatedAt`, and `MimeType` fields of the existing asset table
