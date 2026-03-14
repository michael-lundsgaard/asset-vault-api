using AssetVault.Application.Common.Interfaces;
using AssetVault.Application.UserProfiles.Commands;
using AssetVault.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace AssetVault.UnitTests.Application.UserProfiles;

public class GetOrCreateUserProfileCommandHandlerTests : IDisposable
{
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly GetOrCreateUserProfileCommandHandler _sut;

    public GetOrCreateUserProfileCommandHandlerTests() =>
        _sut = new GetOrCreateUserProfileCommandHandler(_userProfileRepository, _cache);

    public void Dispose() => _cache.Dispose();

    [Fact]
    public async Task Handle_GivenCacheHit_ShouldReturnCachedProfile_WithoutHittingRepository()
    {
        var userId = Guid.NewGuid();
        var cachedProfile = UserProfile.Create(userId, "cached@example.com", "cached");
        _cache.Set(GetOrCreateUserProfileCommand.CacheKey(userId), cachedProfile);

        var command = new GetOrCreateUserProfileCommand(userId, "cached@example.com");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeSameAs(cachedProfile);
        await _userProfileRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenCacheMissAndExistingProfile_ShouldReturnProfile_AndPopulateCache()
    {
        var userId = Guid.NewGuid();
        var existingProfile = UserProfile.Create(userId, "user@example.com", "user");
        _userProfileRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existingProfile);

        var command = new GetOrCreateUserProfileCommand(userId, "user@example.com");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeSameAs(existingProfile);
        _cache.TryGetValue(GetOrCreateUserProfileCommand.CacheKey(userId), out UserProfile? cached);
        cached.Should().BeSameAs(existingProfile);
        await _userProfileRepository.DidNotReceive().AddAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenCacheMissAndNoProfile_ShouldCreateProfile_AddToRepository_AndPopulateCache()
    {
        var userId = Guid.NewGuid();
        _userProfileRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);

        var command = new GetOrCreateUserProfileCommand(userId, "new@example.com");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Id.Should().Be(userId);
        result.Email.Should().Be("new@example.com");
        result.DisplayName.Should().Be("new");
        await _userProfileRepository.Received(1).AddAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>());
        await _userProfileRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _cache.TryGetValue(GetOrCreateUserProfileCommand.CacheKey(userId), out UserProfile? cached);
        cached.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_GivenEmailWithSubdomain_ShouldSetDisplayNameToLocalPart()
    {
        var userId = Guid.NewGuid();
        _userProfileRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);

        var command = new GetOrCreateUserProfileCommand(userId, "john.doe@company.com");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.DisplayName.Should().Be("john.doe");
    }
}
