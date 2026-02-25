---
applyTo: 'tests/**/*.cs'
---

# Test File Instructions

You are writing tests for MediaVault. Follow these conventions exactly.

## Framework stack

- **xUnit** for test runner
- **FluentAssertions** for assertions (`result.Should().Be(...)`)
- **NSubstitute** for mocking (`Substitute.For<IInterface>()`)

## Structure

```csharp
public class [Subject]Tests
{
    private readonly IMock _mock = Substitute.For<IMock>();
    private readonly SubjectClass _sut;

    public [Subject]Tests() => _sut = new SubjectClass(_mock);

    [Fact]
    public async Task [Method]_Given[Condition]_Should[Outcome]()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

## NSubstitute patterns

- Setup return: `_mock.MethodAsync(arg).Returns(value);`
- Capture arg: `_mock.MethodAsync(Arg.Do<T>(x => captured = x));`
- Verify called: `await _mock.Received(1).MethodAsync(Arg.Any<CancellationToken>());`
- Verify not called: `await _mock.DidNotReceive().MethodAsync(...);`

## Do not

- Use real database connections in unit tests
- Use `Moq` — project uses `NSubstitute`
- Assert implementation details — assert observable outcomes
- Leave tests without at least one `Should()` assertion
