# Scaffold New CQRS Feature

Scaffold a complete CQRS feature for MediaVault following the project conventions.

## Input required from me:

- Entity name: [ENTITY]
- Feature description: [DESCRIBE WHAT THIS DOES]

## Generate the following files:

### 1. Command (if write operation)

File: `src/MediaVault.Application/[ENTITY]s/Commands/[Verb][ENTITY]Command.cs`

```csharp
// Command record + Handler in same file
// Handler receives interfaces (IRepository), never DbContext
// Return a result record, not void (even for mutations)
```

### 2. Query (if read operation)

File: `src/MediaVault.Application/[ENTITY]s/Queries/Get[ENTITY]By[X]Query.cs`

```csharp
// Query record includes AssetExpand (or equivalent) parameter
// Handler calls GetByIdWithExpandAsync, maps to response record
// Return type is the Contracts response record
```

### 3. FluentValidation validator

File: `src/MediaVault.Application/[ENTITY]s/Commands/[Verb][ENTITY]CommandValidator.cs`

```csharp
// Validate all command properties
// Use RuleFor chains with meaningful messages
```

### 4. Contract response

File: `src/MediaVault.Contracts/Responses/[ENTITY]Response.cs`

```csharp
// Immutable record
// Nullable fields for expandable relationships
```

### 5. Controller action (add to existing controller or create new)

```csharp
// Thin — only parse request, call mediator, return HTTP result
// Include ProducesResponseType attributes
// Parse ?expand= via ExpandParser if the entity supports expand
```

### 6. Unit test

File: `tests/MediaVault.UnitTests/Application/[ENTITY]s/[Verb][ENTITY]CommandHandlerTests.cs`

```csharp
// Use NSubstitute for mocks
// Follow: {Method}_Given{Condition}_Should{Outcome} naming
// Test the happy path + at least one edge/failure case
```

## Conventions reminder

- Primary constructors for DI
- Always `CancellationToken cancellationToken = default` on async methods
- `DateTime.UtcNow` not `DateTime.Now`
- Repository interface, not DbContext, in handlers
