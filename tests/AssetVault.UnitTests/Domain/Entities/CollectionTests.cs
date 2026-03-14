using AssetVault.Domain.Entities;
using FluentAssertions;

namespace AssetVault.UnitTests.Domain.Entities;

public class CollectionTests
{
    [Fact]
    public void Create_GivenValidArgs_ShouldSetProperties()
    {
        var userId = Guid.NewGuid();

        var collection = Collection.Create(userId, "My Collection", "A description");

        collection.UserId.Should().Be(userId);
        collection.Name.Should().Be("My Collection");
        collection.Description.Should().Be("A description");
    }

    [Fact]
    public void Create_GivenNoDescription_ShouldLeaveDescriptionNull()
    {
        var collection = Collection.Create(Guid.NewGuid(), "No Desc");

        collection.Description.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldSetNewNameAndDescription()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Old Name", "Old description");
        var before = collection.UpdatedAt;

        collection.Update("New Name", "New description");

        collection.Name.Should().Be("New Name");
        collection.Description.Should().Be("New description");
        collection.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_GivenNullDescription_ShouldSetDescriptionToNull()
    {
        var collection = Collection.Create(Guid.NewGuid(), "Name", "Old description");

        collection.Update("Name", null);

        collection.Description.Should().BeNull();
    }
}
