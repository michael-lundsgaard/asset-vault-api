# Tests

Three test projects sit under this folder, each targeting a distinct layer of confidence.

```
tests/
├── AssetVault.UnitTests/           # Fast, no I/O, NSubstitute mocks
├── AssetVault.IntegrationTests/    # Real Postgres (Testcontainers), real HTTP pipeline
└── AssetVault.ArchitectureTests/   # NetArchTest layer-dependency enforcement
```

## Running the tests

```bash
# All projects
dotnet test

# One project at a time
dotnet test tests/AssetVault.UnitTests
dotnet test tests/AssetVault.IntegrationTests
dotnet test tests/AssetVault.ArchitectureTests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

Integration tests spin up a Docker container, so the Docker daemon must be running.

---

## Unit Tests (`AssetVault.UnitTests`)

Handler tests follow a strict, consistent structure:

- **One test class per handler**, one file per class.
- Private readonly mock fields declared inline with `Substitute.For<I...>()`.
- `_sut` wired in the parameterless constructor.
- Method naming: `Handle_Given{Condition}_Should{Outcome}`.
- `CancellationToken.None` passed explicitly to every `Handle(...)` call.

`GetOrCreateUserProfileCommandHandlerTests` uses a **real** `MemoryCache` instance (not a substitute) and implements `IDisposable` to dispose it — consistent with the project instruction that the cache must not be mocked.

---

## Integration Tests (`AssetVault.IntegrationTests`)

### Infrastructure

| File                         | Purpose                                                                                          |
| ---------------------------- | ------------------------------------------------------------------------------------------------ |
| `AssetVaultWebAppFactory.cs` | Owns the Testcontainers Postgres instance; wires up test doubles; exposes `ResetDatabaseAsync()` |
| `TestAuthHandler.cs`         | Replaces Supabase JWT validation with a simple header scheme                                     |
| `FakeStorageService.cs`      | Returns deterministic fake URLs without touching MinIO/R2                                        |

### Container lifecycle

`AssetVaultWebAppFactory` implements `IAsyncLifetime` and is shared across all tests in a test class via `IClassFixture<AssetVaultWebAppFactory>`. The Postgres container starts **once per class**, and EF Core migrations are applied on startup. Stopping the container happens in `DisposeAsync`.

### Per-test isolation (Respawn)

`IntegrationTestBase` implements `IAsyncLifetime`. xUnit creates a **new test-class instance per test method**, so `InitializeAsync` runs before each test and calls `_factory.ResetDatabaseAsync()`.

`ResetDatabaseAsync` uses [Respawn](https://github.com/jbogard/Respawn) to `TRUNCATE` all user tables (excluding `__EFMigrationsHistory`) in milliseconds — far cheaper than re-creating the container or re-running migrations between tests.

```
Test run
  ├─ BeforeClass  → container starts, migrations run         (once)
  ├─ BeforeTest   → Respawn resets all rows                  (per test)
  ├─ Test body    → uses clean DB                            (per test)
  └─ AfterClass   → container stops                         (once)
```

### Authentication

`TestAuthHandler` is registered under the `"Test"` scheme and replaces the default Supabase JWT handler. Format:

```
Authorization: Test {userId}:{email}
```

Call `AuthenticateAs(userId, email)` from within a test to set this header on the shared `HttpClient`. Call it again mid-test to switch identities (e.g. when verifying a 403 for a different owner).

### Storage

`FakeStorageService` replaces `IStorageService`. It returns stable fake URLs in the form:

- Upload: `https://fake-storage.test/upload/{assetId}/{fileName}`
- Download: `https://fake-storage.test/download/{storagePath}`

No network calls are made.

---

## Architecture Tests (`AssetVault.ArchitectureTests`)

Uses [NetArchTest.Rules](https://github.com/BenMorris/NetArchTest) to enforce the Clean Architecture dependency rule at CI time:

- `Domain` → no dependencies on Application, Infrastructure, or API
- `Application` → no dependencies on Infrastructure or API
- `Infrastructure` → no dependency on API
- Handlers must reside in `AssetVault.Application`
- Controllers must reside in `AssetVault.API`
