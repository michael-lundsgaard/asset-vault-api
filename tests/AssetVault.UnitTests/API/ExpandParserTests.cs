using AssetVault.API.Extensions;
using AssetVault.Application.Assets.Queries;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Application.Collections.Queries;
using FluentAssertions;

namespace AssetVault.UnitTests.API;

public class ExpandParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_GivenNullOrWhitespace_ShouldReturnDefault(string? input)
    {
        var result = ExpandParser.Parse<AssetExpand>(input);

        result.Should().Be(AssetExpand.None);
    }

    [Fact]
    public void Parse_GivenSingleValidValue_ShouldReturnCorrectFlag()
    {
        var result = ExpandParser.Parse<AssetExpand>("collections");

        result.Should().Be(AssetExpand.Collections);
    }

    [Fact]
    public void Parse_GivenUppercaseValue_ShouldParseCorrectly()
    {
        var result = ExpandParser.Parse<AssetExpand>("Collections");

        result.Should().Be(AssetExpand.Collections);
    }

    [Fact]
    public void Parse_GivenInvalidValue_ShouldIgnoreItAndReturnDefault()
    {
        var result = ExpandParser.Parse<AssetExpand>("unknownflag");

        result.Should().Be(AssetExpand.None);
    }

    [Fact]
    public void Parse_GivenMixedValidAndInvalidValues_ShouldReturnOnlyValidFlags()
    {
        var result = ExpandParser.Parse<AssetExpand>("collections,invalid");

        result.Should().Be(AssetExpand.Collections);
    }

    [Fact]
    public void Parse_GivenCollectionExpandAssets_ShouldReturnCorrectFlag()
    {
        var result = ExpandParser.Parse<CollectionExpand>("assets");

        result.Should().Be(CollectionExpand.Assets);
    }
}
