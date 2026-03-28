# AssetVault — Copilot Instructions

## Project Overview

AssetVault is a .NET 9 media asset management API. Files are uploaded to Cloudflare R2 (S3-compatible), metadata is stored in PostgreSQL via Supabase. The API will be consumed by a React dashboard.

## Architecture: Clean Architecture + CQRS

**Dependency rule: API → Application → Domain. Infrastructure implements interfaces from Application/Domain. Never reverse this.**

```
AssetVault.Domain         → Zero dependencies. Entities, Value Objects, Domain Events.
AssetVault.Application    → CQRS handlers (MediatR), FluentValidation, interfaces.
AssetVault.Infrastructure → EF Core, S3, repository implementations.
AssetVault.API            → Controllers (thin), middleware, DI wiring.
AssetVault.Contracts      → Request/Response DTOs (public API surface).
```

## Upload Flow (Presigned URL Pattern)

Files never pass through the API server:

1. `POST /api/assets/upload` → `InitiateUploadCommand` creates a `MediaAsset` (status: `Pending`) + calls `IStorageService.GenerateUploadUrlAsync` → returns presigned S3 URL
2. Client uploads directly to MinIO/R2 via the presigned URL
3. `PATCH /api/assets/{id}/confirm` → `ConfirmUploadCommand` calls `asset.MarkAsUploaded()` → status becomes `Active`

## Auth & UserProfile Flow

- Auth uses Supabase JWTs (standard `sub` + `email` claims)
- `UserProfileMiddleware` runs after `UseAuthorization`, resolves `IUserProfileService` per-request from `context.RequestServices`, calls `GetOrCreateAsync(userId, email, ct)`, and stores the result in `HttpContext.Items["UserProfile"]`
- `IUserProfileService.GetOrCreateAsync` checks `IMemoryCache` first (key from `IUserProfileService.CacheKey(userId)`) before hitting the DB — do not remove the cache layer
- `UserProfileService` lives in the **Application** layer and is registered as `Scoped`. It must only be consumed by middleware — never injected into CQRS handlers.
- To invalidate the cache when a profile is updated, use `IUserProfileService.CacheKey(userId)` from your `UpdateUserProfileCommandHandler`
- Controllers access the profile via `HttpContext.GetRequiredUserProfile()` (throws if missing) or `GetUserProfile()` (returns null)
- `IUserProfileService` is resolved from `context.RequestServices` inside the singleton middleware to avoid capturing a scoped service

## CQRS Conventions

- Command + Handler in the same file; same for Query + Handler
- Commands: `Application/{Entity}/Commands/`, queries: `Application/{Entity}/Queries/`
- Handlers use primary constructor injection with interfaces only — never `AppDbContext` directly
- Queries return `AssetResponse?` (contract type); commands return a dedicated result record (not `Unit`)
- Entity not found in a query → return `null` from the handler; controller returns `NotFound()`
- Entity not found in a command → `throw new KeyNotFoundException($"{entity} {id} not found.")`
- After any mutation: call `repository.SaveChangesAsync(cancellationToken)`
- Domain events are raised inside entity methods (e.g. `asset.AddDomainEvent(...)`), never directly from handlers
- Pipeline behaviors: `ValidationBehavior` (FluentValidation) + `LoggingBehavior` — registered in `ApplicationServiceExtensions`

## Expand Pattern

`[Flags] enum AssetExpand` is defined in `Application/Common/Interfaces/IAssetRepository.cs`.

- `ExpandParser.Parse(string?)` in the API layer converts `?expand=collection,tags` into the flags enum
- Flags are passed into the Query record and forwarded to `IAssetRepository.GetByIdAsync(id, expand, ct)`
- Repository conditionally `.Include()`s navigations based on `expand.HasFlag(...)`
- Mapping (`AssetMappings.ToResponse`) uses `expand.HasFlag(...)` to populate nullable response fields — `null` means "not expanded, don't show the field", not "entity missing"

## Mapping Pattern

No AutoMapper. Manual static extension methods in `Application/{Entity}/Mappings/{Entity}Mappings.cs`.

```csharp
// Example: src/AssetVault.Application/Assets/Mappings/AssetMappings.cs
public static AssetResponse ToResponse(this MediaAsset asset, AssetExpand expand) => new(...) { ... };
```

## Domain Model

- Entities use `private` setters + a `private` EF constructor + a `static Create(...)` factory method
- Value Objects (`FileSize`, `StoragePath`) are record types with `private` constructor + `static Create()` factory
- Domain Events: `{Entity}{PastTense}Event` raised in entity methods, dispatched by EF Core interceptor or `SaveChangesAsync` override

## Naming Conventions

- Entities: PascalCase, no suffix (`MediaAsset`, `Collection`, `Tag`)
- Interfaces: `I{Name}` (`IAssetRepository`, `IStorageService`)
- EF Config: `{Entity}Configuration : IEntityTypeConfiguration<{Entity}>`
- Domain Events: `{Entity}{PastTense}Event` (`AssetCreatedEvent`, `AssetUploadedEvent`)

## Key Libraries

- **MediatR 12** — CQRS pipeline
- **FluentValidation 11** — validation (registered via DI, runs in `ValidationBehavior`)
- **EF Core 9 + Npgsql** — Supabase PostgreSQL
- **AWSSDK.S3** — Cloudflare R2 in prod / MinIO locally (`ForcePathStyle = true`, custom `ServiceUrl`)
- **IMemoryCache** — UserProfile cache (built-in .NET, no Redis needed locally)
- **xUnit + FluentAssertions + NSubstitute** — unit tests
- **Testcontainers** — integration tests
- **NetArchTest** — architecture enforcement tests
- **Scalar** — API docs at `/scalar/v1` (replaces Swagger UI)

## Local Dev Setup

```bash
docker compose up -d           # Start MinIO (console: http://localhost:9001, minioadmin/minioadmin)
cd src/AssetVault.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<supabase-connection-string>"
# Private bucket (main assets)
dotnet user-secrets set "Storage:S3:BucketName"        "asset-vault"
dotnet user-secrets set "Storage:S3:ServiceUrl"        "http://localhost:9000"
dotnet user-secrets set "Storage:S3:AccessKeyId"       "minioadmin"
dotnet user-secrets set "Storage:S3:SecretAccessKey"   "minioadmin"
dotnet user-secrets set "Storage:S3:UseHttp"           "true"
# Public bucket (thumbnails + cover images — create in MinIO with public read policy)
dotnet user-secrets set "Storage:S3:PublicBucketName"  "asset-vault-public"
dotnet user-secrets set "Storage:S3:PublicServiceUrl"  "http://localhost:9000"
dotnet user-secrets set "Storage:S3:PublicBaseUrl"     "http://localhost:9000/asset-vault-public"
# Migrations
dotnet ef database update --project src/AssetVault.Infrastructure --startup-project src/AssetVault.API
# Add a new migration
dotnet ef migrations add {Name} --project src/AssetVault.Infrastructure --startup-project src/AssetVault.API
```

## Testing

- Unit tests: test handlers with NSubstitute mocks; use a real `MemoryCache` instance (not mocked) when the handler depends on `IMemoryCache`
- Test class constructor wires up `_sut`; fields are readonly with `Substitute.For<I...>()` inline
- Test method naming: `Handle_Given{Condition}_Should{Outcome}`
- Integration tests: `WebApplicationFactory<Program>` + Testcontainers Postgres
- Architecture tests: NetArchTest enforces layer boundaries

## Code Style

- Nullable reference types enabled everywhere
- Primary constructors preferred for DI
- Records for DTOs, commands, queries, value objects
- `async/await` throughout — always pass and forward `CancellationToken`
- `var` only for LINQ results or when the type is obvious from the RHS
- Keep controllers thin: parse request → call `mediator.Send(...)` → return HTTP result

## What NOT to Do

- Never inject `AppDbContext` directly into handlers — use repository interfaces
- Never put business logic in controllers or middleware
- Never add Infrastructure references in Application or Domain
- Never skip `CancellationToken` on async methods
- Never use `DateTime.Now` — always `DateTime.UtcNow`
- Never use `Moq` — project uses `NSubstitute`
