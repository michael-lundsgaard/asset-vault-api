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

## CQRS Conventions

- Commands: `{Verb}{Entity}Command` + `{Verb}{Entity}CommandHandler` in same file
- Queries: `Get{Entity}By{X}Query` + handler in same file
- Commands go in `Application/{Entity}/Commands/`, queries in `Application/{Entity}/Queries/`
- Handlers take interfaces (e.g. `IAssetRepository`), never `AppDbContext` directly
- Return types from commands: result records. From queries: contract response records.

## Expand Pattern

The `?expand=collection,tags` query param is parsed into `[Flags] enum AssetExpand`.

- Parsing happens in `ExpandParser` in the API layer
- The expand flags are passed into the Query record
- Repository `GetByIdWithExpandAsync` conditionally `.Include()`s based on flags
- Response records have nullable fields for expandable data (null = not expanded)

## Naming Conventions

- Entities: PascalCase, no suffix (`MediaAsset`, `Collection`, `Tag`)
- Value Objects: record types with private ctor + static `Create()` factory
- Domain Events: `{Entity}{Past Tense}Event` (`AssetCreatedEvent`)
- Interfaces: `I{Name}` (`IAssetRepository`, `IStorageService`)
- EF Config: `{Entity}Configuration : IEntityTypeConfiguration<{Entity}>`

## Key Libraries

- **MediatR 12** — CQRS pipeline
- **FluentValidation 11** — validation (registered via DI, runs in `ValidationBehavior`)
- **EF Core 9 + Npgsql** — Supabase PostgreSQL
- **AWSSDK.S3** — Cloudflare R2 (S3-compatible, `ForcePathStyle = true`, custom endpoint)
- **xUnit + FluentAssertions + NSubstitute** — unit tests
- **Testcontainers** — integration tests
- **NetArchTest** — architecture enforcement tests
- **Scalar** — API docs (replaces Swagger UI)

## Testing Rules

- Unit tests: test handlers in isolation, mock with NSubstitute, never use real DB
- Integration tests: use `WebApplicationFactory<Program>`, spin up Postgres via Testcontainers
- Architecture tests: use NetArchTest to enforce layer boundaries
- Test method naming: `{Method}_Given{Condition}_Should{ExpectedOutcome}`

## Code Style

- Nullable reference types enabled everywhere
- Primary constructors preferred for DI (e.g. `class Foo(IBar bar)`)
- Records for DTOs, commands, queries, and value objects
- `async/await` throughout, always pass `CancellationToken`
- No `var` for non-obvious types; `var` fine for LINQ and obvious types
- Keep controllers thin — dispatch to MediatR, return HTTP result

## What NOT to Do

- Never inject `AppDbContext` directly into handlers — use `IAssetRepository`
- Never put business logic in controllers
- Never add Infrastructure references in Application or Domain
- Never skip `CancellationToken` parameters on async methods
- Never use `DateTime.Now` — always `DateTime.UtcNow`
