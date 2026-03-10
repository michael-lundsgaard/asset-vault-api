<!--
SYNC IMPACT REPORT
==================
Version change: (new) → 1.0.0
Modified principles: N/A — initial ratification, all principles are new
Added sections: Core Principles (I–V), Technology Stack Constraints, Development Workflow, Governance
Removed sections: None
Templates requiring updates:
  ✅ .specify/templates/plan-template.md — Constitution Check gate updated
  ✅ .specify/templates/spec-template.md — no structural changes required
  ✅ .specify/templates/tasks-template.md — path conventions confirmed for .NET layout
Deferred TODOs: None
-->

# AssetVault Constitution

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)

Dependencies MUST flow inward: API → Application → Domain. Infrastructure implements
interfaces defined in Application or Domain — never the reverse.

- `AssetVault.Domain` has zero external dependencies. Entities, Value Objects, Domain Events only.
- `AssetVault.Application` depends only on Domain. MUST NOT reference Infrastructure or API.
- `AssetVault.Infrastructure` implements Application/Domain interfaces (repositories, storage, EF Core).
- `AssetVault.API` wires DI and exposes HTTP endpoints. MUST NOT contain business logic.
- `AssetVault.Contracts` holds request/response DTOs only — no logic, no domain references.
- Layer boundaries are enforced by NetArchTest architecture tests and MUST remain green on every PR.

### II. CQRS via MediatR (NON-NEGOTIABLE)

Every user operation MUST be expressed as a MediatR Command or Query dispatched from the controller.

- Business logic MUST live exclusively in Application layer handlers — never in controllers or Infrastructure.
- Controllers MUST be thin: parse request → `mediator.Send(command/query)` → return HTTP result.
- Every Command and Query MUST have a corresponding FluentValidation validator registered via DI.
- Command + Handler in the same file; Query + Handler in the same file.
- Commands live in `Application/{Entity}/Commands/`; Queries in `Application/{Entity}/Queries/`.
- Handlers use primary constructor injection with interfaces only — never `AppDbContext` directly.
- Pipeline behaviors `ValidationBehavior` and `LoggingBehavior` MUST remain registered.

### III. Test Quality Gate

All new handlers MUST be covered by unit tests before a feature is considered complete.

- Unit tests: xUnit + FluentAssertions + NSubstitute. NEVER use Moq.
- Integration tests: `WebApplicationFactory<Program>` + Testcontainers (PostgreSQL).
- Architecture boundary tests: NetArchTest — all layer rules MUST pass.
- Test naming convention: `Handle_Given{Condition}_Should{Outcome}`.
- Use real `MemoryCache` instances in tests (not mocked) when the handler depends on `IMemoryCache`.
- Test class constructor wires up `_sut`; fields are `readonly` with `Substitute.For<I...>()` inline.

### IV. Presigned URL Storage Pattern

Files MUST never pass through the API server.

- All file storage operations MUST follow: initiate upload (create entity, generate presigned URL)
  → client uploads directly to S3/R2 → confirm upload (mark entity Active).
- `IStorageService` in Application abstracts all S3/MinIO concerns; Infrastructure implements it.
- New storage-related features MUST extend this pattern rather than introducing direct upload paths.

### V. Domain Integrity

The domain model enforces its own invariants; no layer above may violate them.

- Entities MUST use `private` setters, a `private` EF Core constructor, and a `static Create(...)` factory.
- State transitions MUST be encapsulated in entity methods (e.g., `asset.MarkAsUploaded()`), never
  performed directly from handlers or controllers.
- Domain events MUST be raised inside entity methods and dispatched via the EF Core interceptor or
  `SaveChangesAsync` override — never directly from handlers.
- Value Objects (`FileSize`, `StoragePath`) are record types with `private` constructor + `static Create()`.
- NEVER use `DateTime.Now` — always `DateTime.UtcNow`.

## Technology Stack Constraints

The following stack choices are fixed and MUST NOT be replaced without a constitution amendment.

| Concern            | Technology                                              |
| ------------------ | ------------------------------------------------------- |
| Runtime            | .NET 9 / ASP.NET Core 9                                 |
| CQRS               | MediatR 12                                              |
| Validation         | FluentValidation 11                                     |
| Database ORM       | EF Core 9 + Npgsql → Supabase (PostgreSQL)              |
| Storage            | AWS SDK S3 → MinIO (local) / Cloudflare R2 (production) |
| Auth               | Supabase JWTs (standard `sub` + `email` claims)         |
| API docs           | Scalar at `/scalar/v1`                                  |
| Unit Tests         | xUnit + FluentAssertions + NSubstitute                  |
| Integration Tests  | Testcontainers                                          |
| Architecture Tests | NetArchTest                                             |

Additional constraints:

- No AutoMapper. Mapping MUST use manual static extension methods in `Application/{Entity}/Mappings/`.
- `IMemoryCache` (built-in .NET) for UserProfile caching — no Redis required locally.
- MinIO runs via `docker compose up -d`; S3 client uses `ForcePathStyle = true` with custom `ServiceUrl`.

## Development Workflow

Follow this checklist when implementing any new feature:

1. **Domain**: Add or extend entity in `AssetVault.Domain/Entities/`. Use `static Create(...)` factory.
2. **Repository interface**: Define in `AssetVault.Application/Common/Interfaces/`.
3. **EF Config**: Add `IEntityTypeConfiguration<TEntity>` in Infrastructure.
4. **Handler(s)**: Create Command/Query + Handler + FluentValidation validator in `Application/{Entity}/`.
5. **Mapping**: Add `ToResponse(...)` extension in `Application/{Entity}/Mappings/`.
6. **Controller**: Add thin endpoint in `AssetVault.API/Controllers/`.
7. **Tests**: Unit tests for handlers; integration test for the endpoint if applicable.
8. **Migration**: `dotnet ef migrations add {Name} --project src/AssetVault.Infrastructure --startup-project src/AssetVault.API`

Auth access pattern in controllers:

- Use `HttpContext.GetRequiredUserProfile()` — throws if profile missing.
- Use `HttpContext.GetUserProfile()` — returns null if not present.

## Governance

This constitution supersedes all other practices documented in the repository. Any contradiction between
this document and other guidelines MUST be resolved in favour of this constitution.

Amendment procedure:

1. Open a PR with the proposed change to `.specify/memory/constitution.md`.
2. Increment the version per semantic versioning rules (MAJOR: principle removal/redefinition;
   MINOR: new principle or material expansion; PATCH: clarifications and wording).
3. Update the Sync Impact Report comment at the top of this file.
4. Propagate changes to affected templates (plan-template, spec-template, tasks-template).
5. Include a migration note if any existing features are impacted.

All PRs MUST keep NetArchTest architecture tests green. Complexity beyond what the architecture defines
MUST be justified in the PR description and tracked in plan.md's Complexity Tracking table.

Refer to `.github/copilot-instructions.md` for runtime development guidance and conventions.

**Version**: 1.0.0 | **Ratified**: 2026-03-10 | **Last Amended**: 2026-03-10
