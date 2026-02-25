# Generate a Conventional Commit Message

Inspect the staged changes (or all unstaged changes if nothing is staged) and write a commit message following the Conventional Commits 1.0 spec.

## Format

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

## Types

| Type       | When to use                                                    |
| ---------- | -------------------------------------------------------------- |
| `feat`     | A new feature visible to consumers of the API                  |
| `fix`      | A bug fix                                                      |
| `refactor` | Code restructure with no behaviour change                      |
| `perf`     | Performance improvement                                        |
| `test`     | Adding or fixing tests only                                    |
| `build`    | Changes to the build system or dependencies (`.csproj`, NuGet) |
| `ci`       | CI/CD pipeline changes                                         |
| `docs`     | Documentation only                                             |
| `chore`    | Housekeeping — no production code change                       |

## Scopes (use the layer or feature area)

Examples: `assets`, `collections`, `tags`, `storage`, `persistence`, `api`, `contracts`, `domain`, `infra`

## Rules

- Description: imperative mood, lowercase, no trailing period, ≤72 chars
- Body: explain _why_, not _what_ — wrap at 72 chars
- Breaking changes: add `!` after type/scope **and** a `BREAKING CHANGE:` footer
- Reference issues in the footer: `Closes #123`

## Examples

```
feat(assets): add expand query param for collections and tags

fix(storage): handle missing bucket error with NotFoundException

refactor(assets): extract GetByIdWithExpandAsync into repository

test(assets): add unit tests for GetAssetByIdQueryHandler

build: upgrade AWSSDK.S3 to 4.x

feat(api)!: rename /assets/{id} response field url to storageUrl

BREAKING CHANGE: consumers must update to use `storageUrl` instead of `url`
```

## What to do

1. Review the diff of the current changes in the repository.
2. Determine the correct type and scope.
3. Output **only** the commit message — no explanation, no markdown fences.
