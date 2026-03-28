# AssetVault API

Media asset management API built with .NET 9, Supabase (PostgreSQL), and MinIO (S3-compatible local storage).

## Architecture

Clean Architecture + CQRS via MediatR. Dependency rule flows inward:

```
API → Application → Domain
         ↑
  Infrastructure (implements interfaces defined in Application)
```

## Tech Stack

| Layer      | Technology                                                        |
| ---------- | ----------------------------------------------------------------- |
| API        | ASP.NET Core 9, Scalar (OpenAPI)                                  |
| CQRS       | MediatR 12                                                        |
| Validation | FluentValidation 11                                               |
| Database   | EF Core 9 + Npgsql → Supabase (PostgreSQL)                        |
| Storage    | AWS SDK S3 → MinIO (local) / Cloudflare R2 (production)           |
| Tests      | xUnit, FluentAssertions, NSubstitute, Testcontainers, NetArchTest |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for MinIO)
- [dotnet-ef CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) — `dotnet tool install --global dotnet-ef`

## Getting Started

### 1. Start MinIO (local S3)

```bash
docker compose up -d
```

MinIO console available at `http://localhost:9001` (minioadmin / minioadmin).

### 2. Configure user secrets

```bash
cd src/AssetVault.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SUPABASE_CONNECTION_STRING"

# Private bucket — main asset uploads
dotnet user-secrets set "Storage:S3:BucketName"      "asset-vault"
dotnet user-secrets set "Storage:S3:AccountId"       "local"
dotnet user-secrets set "Storage:S3:AccessKeyId"     "minioadmin"
dotnet user-secrets set "Storage:S3:SecretAccessKey" "minioadmin"
dotnet user-secrets set "Storage:S3:ServiceUrl"      "http://localhost:9000"
dotnet user-secrets set "Storage:S3:UseHttp"         "true"

# Public bucket — thumbnails and cover images (create this bucket in MinIO with public access policy)
dotnet user-secrets set "Storage:S3:PublicBucketName"  "asset-vault-public"
dotnet user-secrets set "Storage:S3:PublicServiceUrl"  "http://localhost:9000"
dotnet user-secrets set "Storage:S3:PublicBaseUrl"     "http://localhost:9000/asset-vault-public"
```

### 3. Run migrations

```bash
dotnet ef database update \
  --project src/AssetVault.Infrastructure \
  --startup-project src/AssetVault.API
```

> To create a new migration after a schema change:
> `dotnet ef migrations add {MigrationName} --project src/AssetVault.Infrastructure --startup-project src/AssetVault.API`

### 4. Run the API

```bash
dotnet run --project src/AssetVault.API
```

API docs available at `http://localhost:{port}/scalar/v1`

### 5. Run tests

> TO BE IMPLEMENTED

```bash
dotnet test                                       # all tests
dotnet test tests/AssetVault.UnitTests            # unit only
dotnet test tests/AssetVault.ArchitectureTests    # architecture rules
```

---

## Upload Flow

The API uses a presigned URL pattern — files go directly to storage, never through the API server:

```
1. POST   /api/assets/upload         → creates asset record (Pending) + returns presigned S3 URL
2. PUT    {presignedUrl}             → client uploads file directly to MinIO/R2
3. PATCH  /api/assets/{id}/confirm   → marks asset as Active in DB
4. GET    /api/assets/{id}           → verify
```

## Key API Endpoints

```
GET    /api/assets                               → List all assets
GET    /api/assets?expand=collection,tags        → List assets with related data
GET    /api/assets/{id}                          → Get asset by ID
GET    /api/assets/{id}?expand=collection,tags   → Get asset by ID with related data
POST   /api/assets/upload                        → Initiate upload (returns presigned URL)
PATCH  /api/assets/{id}/confirm                  → Confirm upload complete
```

---

## Local vs Production Storage

The storage service is S3-compatible — swapping from local MinIO to production requires only config changes, zero code changes.

**Private bucket** (`BucketName`) — main asset files, accessed via presigned URLs only.

| Setting           | Local (MinIO)           | Production (Cloudflare R2)                     |
| ----------------- | ----------------------- | ---------------------------------------------- |
| `BucketName`      | `asset-vault`           | Your private R2 bucket name                    |
| `ServiceUrl`      | `http://localhost:9000` | `https://<accountId>.r2.cloudflarestorage.com` |
| `AccountId`       | `local`                 | Your Cloudflare Account ID                     |
| `UseHttp`         | `true`                  | `false`                                        |
| `AccessKeyId`     | `minioadmin`            | R2 API Token — Access Key ID                   |
| `SecretAccessKey` | `minioadmin`            | R2 API Token — Secret Access Key               |

**Public bucket** (`PublicBucketName`) — thumbnails and cover images, served without signing.
In production: use Cloudflare R2 **Public Development URL** (or a custom domain) — no WAF/custom domain required for dev.
In MinIO: create the bucket and set its access policy to **public read**.

| Setting            | Local (MinIO)                              | Production (Cloudflare R2)                     |
| ------------------ | ------------------------------------------ | ---------------------------------------------- |
| `PublicBucketName` | `asset-vault-public`                       | Your public R2 bucket name                     |
| `PublicServiceUrl` | `http://localhost:9000`                    | `https://<accountId>.r2.cloudflarestorage.com` |
| `PublicBaseUrl`    | `http://localhost:9000/asset-vault-public` | `https://pub-<token>.r2.dev` (Public Dev URL)  |

---

## Copilot Agent Setup (VSCode)

The project includes AI coding agent configuration:

| File                                            | Purpose                                    |
| ----------------------------------------------- | ------------------------------------------ |
| `.github/copilot-instructions.md`               | Always-on project context for Copilot Chat |
| `.github/prompts/scaffold-feature.prompt.md`    | Invoke via `#scaffold-feature` in chat     |
| `.github/prompts/write-tests.prompt.md`         | Invoke via `#write-tests` in chat          |
| `.github/prompts/add-expand.prompt.md`          | Invoke via `#add-expand` in chat           |
| `.github/prompts/conventional-commit.prompt.md` | Invoke via `#conventional-commit` in chat  |
| `.github/instructions/handlers.instructions.md` | Auto-activates on `*Handler.cs` files      |
| `.github/instructions/tests.instructions.md`    | Auto-activates on `tests/**/*.cs` files    |

### How to use prompts in Copilot Chat

```
@workspace #scaffold-feature I need to add tagging to collections
@workspace #write-tests [with the handler file open]
```
