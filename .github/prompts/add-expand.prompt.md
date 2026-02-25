# Add ?expand= Support to a Query

Add expand parameter support to an existing query following the AssetVault pattern.

## The Pattern

1. **Define flags enum** (if not already in `IAssetRepository.cs`):

```csharp
[Flags]
public enum [Entity]Expand
{
    None = 0,
    [RelatedEntity] = 1,
    [AnotherRelated] = 2,
    All = [RelatedEntity] | [AnotherRelated]
}
```

2. **Update Query record** to include expand parameter:

```csharp
public record Get[Entity]ByIdQuery(Guid Id, [Entity]Expand Expand = [Entity]Expand.None)
```

3. **Update Repository interface** with expand overload:

```csharp
Task<[Entity]?> GetByIdWithExpandAsync(Guid id, [Entity]Expand expand, CancellationToken ct = default);
```

4. **Implement in Repository** using conditional `.Include()`:

```csharp
if (expand.HasFlag([Entity]Expand.[RelatedEntity]))
    query = query.Include(x => x.[RelatedEntity]);
```

5. **Update Handler** to pass expand to repo and conditionally map response:

```csharp
[RelatedEntity]: request.Expand.HasFlag([Entity]Expand.[RelatedEntity]) && entity.[RelatedEntity] is not null
    ? Map(entity.[RelatedEntity])
    : null
```

6. **Update Controller** to parse `?expand=` query string:

```csharp
[FromQuery] string? expand
// Parse with ExpandParser.Parse(expand) → cast to [Entity]Expand
```

7. **Update Response record** — expandable fields should be nullable:

```csharp
[RelatedEntity]Summary? [RelatedEntity] = null  // null = not requested
```

## Now apply this pattern to: [ENTITY / ENDPOINT]
