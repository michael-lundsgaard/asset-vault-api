using AssetVault.Domain.ValueObjects;

namespace AssetVault.UnitTests.Domain.ValueObjects;

public class StoragePathTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_GivenNullOrWhitespacePath_ShouldThrowArgumentException(string? path)
    {
        var act = () => StoragePath.Create(path!);

        act.Should().Throw<ArgumentException>().WithParameterName("path");
    }

    [Fact]
    public void Create_GivenValidPath_ShouldSetValue()
    {
        var result = StoragePath.Create("uploads/abc123/file.jpg");

        result.Value.Should().Be("uploads/abc123/file.jpg");
    }

    [Fact]
    public void Create_GivenValidPath_ShouldReturnLastSegmentAsBucketKey()
    {
        var result = StoragePath.Create("uploads/abc123/file.jpg");

        result.BucketKey.Should().Be("file.jpg");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        const string path = "uploads/abc/test.png";

        var result = StoragePath.Create(path);

        result.ToString().Should().Be(path);
    }
}
