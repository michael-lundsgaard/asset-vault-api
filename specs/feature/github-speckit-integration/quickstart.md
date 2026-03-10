# Quickstart: Adding an Analytics Query

**Feature**: Asset Analytics Stats  
**Date**: 2026-03-10

This guide shows the steps to add a new analytics query endpoint, following the same pattern used for the three endpoints in this feature.

---

## Prerequisites

- Docker is running (`docker compose up -d` for MinIO)
- Supabase connection string is set in user-secrets
- Existing test suite is green: `dotnet test`

---

## Step-by-Step: Adding a New Analytics Query

Use **"Get total tag count by user"** as the example throughout.

### 1. Define the internal result record (Application layer)

```csharp
// src/AssetVault.Application/Analytics/
public record TagCountSummary(int TotalTags);
```

### 2. Add a method to `IAnalyticsRepository`

```csharp
// src/AssetVault.Application/Common/Interfaces/IAnalyticsRepository.cs
Task<TagCountSummary> GetTagCountAsync(
    Guid userId, CancellationToken cancellationToken = default);
```

### 3. Implement the method in `AnalyticsRepository`

```csharp
// src/AssetVault.Infrastructure/Persistence/Repositories/AnalyticsRepository.cs
public async Task<TagCountSummary> GetTagCountAsync(
    Guid userId, CancellationToken cancellationToken = default)
{
    var count = await context.Assets
        .AsNoTracking()
        .Where(a => a.UserId == userId && a.Status == AssetStatus.Active)
        .SelectMany(a => a.Tags)
        .CountAsync(cancellationToken);

    return new TagCountSummary(count);
}
```

### 4. Add the contract response DTO

```csharp
// src/AssetVault.Contracts/Responses/Analytics/AnalyticsResponses.cs
public record TagCountResponse(int TotalTags);
```

### 5. Create the Query + Handler + Validator (same file)

```csharp
// src/AssetVault.Application/Analytics/Queries/GetTagCountQuery.cs
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Analytics;
using MediatR;

namespace AssetVault.Application.Analytics.Queries;

public record GetTagCountQuery(Guid UserId) : IRequest<TagCountResponse>;

// No validator needed — no parameters to validate beyond UserId (provided from auth context)

public class GetTagCountQueryHandler(IAnalyticsRepository analyticsRepository)
    : IRequestHandler<GetTagCountQuery, TagCountResponse>
{
    public async Task<TagCountResponse> Handle(
        GetTagCountQuery request, CancellationToken cancellationToken)
    {
        var result = await analyticsRepository.GetTagCountAsync(request.UserId, cancellationToken);
        return new TagCountResponse(result.TotalTags);
    }
}
```

### 6. Add the endpoint to `AnalyticsController`

```csharp
// src/AssetVault.API/Controllers/AnalyticsController.cs
[HttpGet("tags")]
[ProducesResponseType(typeof(TagCountResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> GetTagCount(CancellationToken cancellationToken)
{
    var userId = HttpContext.GetRequiredUserProfile().Id;
    var result = await mediator.Send(new GetTagCountQuery(userId), cancellationToken);
    return Ok(result);
}
```

### 7. Write unit tests for the handler

```csharp
// tests/AssetVault.UnitTests/Analytics/GetTagCountQueryHandlerTests.cs
public class GetTagCountQueryHandlerTests
{
    private readonly IAnalyticsRepository _repository = Substitute.For<IAnalyticsRepository>();
    private readonly GetTagCountQueryHandler _sut;

    public GetTagCountQueryHandlerTests()
    {
        _sut = new GetTagCountQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_GivenUserWithTags_ShouldReturnTotalCount()
    {
        _repository.GetTagCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new TagCountSummary(7));

        var result = await _sut.Handle(new GetTagCountQuery(Guid.NewGuid()), default);

        result.TotalTags.Should().Be(7);
    }
}
```

### 8. Run tests

```bash
dotnet test
```

---

## Key Conventions to Follow

| Convention                    | Detail                                                                        |
| ----------------------------- | ----------------------------------------------------------------------------- |
| Query + Handler in same file  | See `GetAnalyticsSummaryQuery.cs` for reference                               |
| Validator in same file        | Only where query has parameters (e.g. date range on `GetUploadActivityQuery`) |
| Primary constructor injection | `GetTagCountQueryHandler(IAnalyticsRepository analyticsRepository)`           |
| No `AppDbContext` in handlers | Only use `IAnalyticsRepository`                                               |
| CancellationToken forwarded   | Always pass `cancellationToken` through to repository                         |
| UTCNow                        | Use `DateTime.UtcNow` — never `DateTime.Now`                                  |
| Auth                          | Get `UserId` from `HttpContext.GetRequiredUserProfile().Id` in controller     |

---

## File Locations Reference

```
src/AssetVault.Application/Analytics/
├── Queries/
│   ├── GetAnalyticsSummaryQuery.cs
│   ├── GetUploadActivityQuery.cs
│   └── GetAssetCategoriesQuery.cs
└── (MimeCategoryResolver.cs, MimeTypeCategory.cs)

src/AssetVault.Application/Common/Interfaces/
└── IAnalyticsRepository.cs

src/AssetVault.Contracts/Responses/Analytics/
└── AnalyticsResponses.cs

src/AssetVault.Infrastructure/Persistence/Repositories/
└── AnalyticsRepository.cs

src/AssetVault.API/Controllers/
└── AnalyticsController.cs

tests/AssetVault.UnitTests/Analytics/
├── GetAnalyticsSummaryQueryHandlerTests.cs
├── GetUploadActivityQueryHandlerTests.cs
└── GetAssetCategoriesQueryHandlerTests.cs
```
