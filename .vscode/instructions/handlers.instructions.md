---
applyTo: '**/*Handler.cs'
---

# Handler File Instructions

You are editing a MediatR command or query handler in MediaVault.

## Strict rules for this file type:

- The **Command/Query record** and its **Handler** must be in the **same file**
- Handler class uses **primary constructor** for dependency injection
- Handler dependencies must be **interfaces only** — never `AppDbContext` directly
- Always accept and forward `CancellationToken cancellationToken` to all async calls
- Use `DateTime.UtcNow` never `DateTime.Now`
- Commands should raise **domain events** via the entity (not directly from handler)
- After mutations, always call `SaveChangesAsync(cancellationToken)`

## Naming

- Command: `{Verb}{Entity}Command` (e.g. `DeleteAssetCommand`)
- Query: `Get{Entity}By{Criterion}Query` (e.g. `GetAssetByIdQuery`)
- Handler: same name + `Handler` suffix

## Return types

- Queries → return a record from `MediaVault.Contracts.Responses`
- Commands → return a result record (not `Unit` unless truly nothing to return)
- If entity not found → `throw new KeyNotFoundException($"{entity} {id} not found.")`
