# Write Unit Tests for a MediatR Handler

Write comprehensive unit tests for the selected handler following MediaVault conventions.

## Rules

- Test framework: **xUnit**
- Assertions: **FluentAssertions**
- Mocking: **NSubstitute** (`Substitute.For<IInterface>()`)
- Handler instantiated as `_sut` (System Under Test)
- All mocks declared as private readonly fields
- Test naming: `Handle_Given{Condition}_Should{Outcome}`

## Test structure template

```csharp
public class [Handler]Tests
{
    // Arrange mocks
    private readonly IDependency _dependency = Substitute.For<IDependency>();
    private readonly HandlerClass _sut;

    public [Handler]Tests()
    {
        _sut = new HandlerClass(_dependency);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldCreateEntityAndReturnResult()
    {
        // Arrange
        // Act
        // Assert — FluentAssertions
    }
}
```

## Coverage to aim for

1. **Happy path** — valid input produces expected output
2. **Side effects** — verify mocks received expected calls (`.Received(1).MethodAsync(...)`)
3. **Domain state** — capture entity passed to repo, assert its state
4. **Not found** — when repo returns null, handler throws expected exception
5. **Validation edge** — if command has constraints, test boundary values

## For the currently selected handler file, generate all of the above.
