using AssetVault.Domain.Entities;
using FluentAssertions;

namespace AssetVault.UnitTests.Domain.Entities;

public class UserProfileTests
{
    [Fact]
    public void Create_ShouldSetIdToProvidedUserId()
    {
        var userId = Guid.NewGuid();

        var profile = UserProfile.Create(userId, "user@example.com", "user");

        profile.Id.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldSetEmailAndDisplayName()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "user@example.com", "user");

        profile.Email.Should().Be("user@example.com");
        profile.DisplayName.Should().Be("user");
    }
}
