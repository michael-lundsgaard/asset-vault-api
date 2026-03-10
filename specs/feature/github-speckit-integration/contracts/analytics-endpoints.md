# API Contract: Analytics Endpoints

**Feature**: Asset Analytics Stats  
**Base path**: `/api/analytics`  
**Auth**: All endpoints require a valid Supabase JWT (`Authorization: Bearer <token>`). Returns `401 Unauthorized` when unauthenticated.

---

## 1. GET /api/analytics/summary

Returns a high-level snapshot of the authenticated user's media library.

### Request

```
GET /api/analytics/summary
Authorization: Bearer <jwt>
```

No query parameters.

### Response — 200 OK

```json
{
    "totalAssets": 42,
    "totalStorageBytes": 1073741824
}
```

| Field               | Type    | Description                                        |
| ------------------- | ------- | -------------------------------------------------- |
| `totalAssets`       | integer | Count of confirmed (`Active`) assets owned by user |
| `totalStorageBytes` | integer | Sum of file sizes in bytes for those assets        |

When the user has no assets: `{ "totalAssets": 0, "totalStorageBytes": 0 }`.

### Error Responses

| Status | Condition              |
| ------ | ---------------------- |
| `401`  | Missing or invalid JWT |

---

## 2. GET /api/analytics/activity

Returns upload counts grouped by calendar day (UTC) for the authenticated user.

### Request

```
GET /api/analytics/activity?from=2026-01-01&to=2026-01-31
Authorization: Bearer <jwt>
```

| Parameter | Type                            | Required | Description                                               |
| --------- | ------------------------------- | -------- | --------------------------------------------------------- |
| `from`    | `date` (ISO 8601, `YYYY-MM-DD`) | No       | Start of date range (inclusive). Defaults to 30 days ago. |
| `to`      | `date` (ISO 8601, `YYYY-MM-DD`) | No       | End of date range (inclusive). Defaults to today (UTC).   |

**Default range**: last 30 calendar days (today inclusive).  
**Maximum range**: 365 days. Exceeding this returns `400 Bad Request`.

### Response — 200 OK

```json
{
  "from": "2026-01-01",
  "to": "2026-01-31",
  "activity": [
    { "date": "2026-01-01", "count": 3 },
    { "date": "2026-01-02", "count": 0 },
    { "date": "2026-01-03", "count": 1 },
    ...
  ]
}
```

| Field              | Type    | Description                                          |
| ------------------ | ------- | ---------------------------------------------------- |
| `from`             | string  | Actual start date used (echoed back, ISO 8601)       |
| `to`               | string  | Actual end date used (echoed back, ISO 8601)         |
| `activity`         | array   | One entry per calendar day in `[from, to]` inclusive |
| `activity[].date`  | string  | Calendar day (UTC), ISO 8601 `YYYY-MM-DD`            |
| `activity[].count` | integer | Number of Active assets uploaded on this day         |

All days in the range are returned, including days with `count: 0`.

### Error Responses

| Status | Condition                   |
| ------ | --------------------------- |
| `400`  | `from` is later than `to`   |
| `400`  | Date range exceeds 365 days |
| `401`  | Missing or invalid JWT      |

**400 response body**:

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "To": ["'To' must be greater than or equal to 'From'."]
    }
}
```

---

## 3. GET /api/analytics/categories

Returns asset counts broken down by MIME type category for the authenticated user.

### Request

```
GET /api/analytics/categories
Authorization: Bearer <jwt>
```

No query parameters.

### Response — 200 OK

```json
{
    "categories": [
        { "category": "image", "count": 15 },
        { "category": "video", "count": 4 },
        { "category": "audio", "count": 0 },
        { "category": "document", "count": 2 },
        { "category": "other", "count": 1 }
    ]
}
```

All five categories are always returned, even when `count` is `0`.

| Field                   | Type    | Description                                            |
| ----------------------- | ------- | ------------------------------------------------------ |
| `categories`            | array   | Five entries — one per MIME type category              |
| `categories[].category` | string  | One of: `image`, `video`, `audio`, `document`, `other` |
| `categories[].count`    | integer | Number of Active assets in this category               |

**Category mapping**:

| Content-Type prefix / pattern                                          | Category   |
| ---------------------------------------------------------------------- | ---------- |
| `image/*`                                                              | `image`    |
| `video/*`                                                              | `video`    |
| `audio/*`                                                              | `audio`    |
| `application/pdf`, `text/*`, `application/msword`, `application/vnd.*` | `document` |
| Everything else                                                        | `other`    |

### Error Responses

| Status | Condition              |
| ------ | ---------------------- |
| `401`  | Missing or invalid JWT |

---

## MediatR Query Types

| HTTP Endpoint                 | MediatR Query              | Return Type                |
| ----------------------------- | -------------------------- | -------------------------- |
| GET /api/analytics/summary    | `GetAnalyticsSummaryQuery` | `AnalyticsSummaryResponse` |
| GET /api/analytics/activity   | `GetUploadActivityQuery`   | `UploadActivityResponse`   |
| GET /api/analytics/categories | `GetAssetCategoriesQuery`  | `AssetCategoriesResponse`  |
