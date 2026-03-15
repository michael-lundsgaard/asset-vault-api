using AssetVault.Application.Common.Interfaces;
using AssetVault.Application.UserProfiles;
using Microsoft.Extensions.Caching.Memory;

namespace AssetVault.UnitTests.Application.UserProfiles;

public class UserProfileServiceTests : IDisposable
{
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly UserProfileService _sut;

    public UserProfileServiceTests() =>
        _sut = new UserProfileService(_userProfileRepository, _cache);

    public void Dispose() => _cache.Dispose();

    [Fact]
    public async Task GetOrCreateAsync_GivenCacheHit_ShouldReturnCachedProfile_WithoutHittingRepository()
    {
        var userId = Guid.NewGuid();
        var cachedProfile = UserProfile.Create(userId, "cached@example.com", "cached");
        _cache.Set(IUserProfileService.CacheKey(userId), cachedProfile);

        var result = await _sut.GetOrCreateAsync(userId, "cached@example.com");

        result.Should().BeSameAs(cachedProfile);
        await _userProfileRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_GivenCacheMissAndExistingProfile_ShouldReturnProfile_AndPopulateCache()
    {
        var userId = Guid.NewGuid();
        var existingProfile = UserProfile.Create(userId, "user@example.com", "user");
        _userProfileRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existingProfile);

        var result = await _sut.GetOrCreateAsync(userId, "user@example.com");

        result.Should().BeSameAs(existingProfile);
        _cache.TryGetValue(IUserProfileService.CacheKey(userId), out UserProfile? cached);
        cached.Should().BeSameAs(existingProfile);
        await _userProfileRepository.DidNotReceive().AddAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_GivenCacheMissAndNoProfile_ShouldCreateProfile_AddToRepository_AndPopulateCache()
    {
        var userId = Guid.NewGuid();
        _userProfileRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);

        var result = await _sut.GetOrCreateAsync(userId, "new@example.com");

        result.Id.Should().Be(userId);
        result.Email.Should().Be("new@example.com");
        result.DisplayName.Should().Be("new");
        await _userProfileRepository.Received(1).AddAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>());
        await _userProfileRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _cache.TryGetValue(IUserProfileService.CacheKey(userId), out UserProfile? cached);
        cached.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_GivenEmailWithSubdomain_ShouldSetDisplayNameToLocalPart()
    {
        var userId = Guid.NewGuid();
        _userProfileRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);

        var result = await _sut.GetOrCreateAsync(userId, "john.doe@company.com");

        result.DisplayName.Should().Be("john.doe");
    }
}
