using AssetVault.Domain.ValueObjects;

namespace AssetVault.UnitTests.Domain.ValueObjects;

public class FileSizeTests
{
    [Fact]
    public void Create_GivenNegativeBytes_ShouldThrowArgumentException()
    {
        var act = () => FileSize.Create(-1);

        act.Should().Throw<ArgumentException>().WithParameterName("bytes");
    }

    [Fact]
    public void Create_GivenZeroBytes_ShouldSucceed()
    {
        var result = FileSize.Create(0);

        result.Bytes.Should().Be(0);
    }

    [Fact]
    public void Create_GivenPositiveBytes_ShouldReturnCorrectDerivedProperties()
    {
        const long bytes = 2_097_152; // 2 MB

        var result = FileSize.Create(bytes);

        result.Bytes.Should().Be(bytes);
        result.Kilobytes.Should().BeApproximately(2048.0, 0.01);
        result.Megabytes.Should().BeApproximately(2.0, 0.01);
        result.Gigabytes.Should().BeApproximately(2.0 / 1024, 0.0001);
    }

    [Fact]
    public void ToString_GivenFileSizeLessThanOneMb_ShouldFormatAsKilobytes()
    {
        var size = FileSize.Create(512 * 1024); // 512 KB

        size.ToString().Should().Contain("KB");
    }

    [Fact]
    public void ToString_GivenFileSizeAtLeastOneMb_ShouldFormatAsMegabytes()
    {
        var size = FileSize.Create(5 * 1024 * 1024); // 5 MB

        size.ToString().Should().Contain("MB");
    }

    [Fact]
    public void ToString_GivenFileSizeAtLeastOneGb_ShouldFormatAsGigabytes()
    {
        var size = FileSize.Create(2L * 1024 * 1024 * 1024); // 2 GB

        size.ToString().Should().Contain("GB");
    }
}
