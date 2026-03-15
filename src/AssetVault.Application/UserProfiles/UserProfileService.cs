using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace AssetVault.Application.UserProfiles
{
    public class UserProfileService(
        IUserProfileRepository userProfileRepository,
        IMemoryCache cache
    ) : IUserProfileService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        /// <inheritdoc/>
        public async Task<UserProfile> GetOrCreateAsync(
            Guid userId,
            string email,
            CancellationToken cancellationToken = default)
        {
            string cacheKey = IUserProfileService.CacheKey(userId);

            if (cache.TryGetValue(cacheKey, out UserProfile? cached) && cached is not null)
                return cached;

            var profile = await userProfileRepository.GetByIdAsync(userId, cancellationToken);

            if (profile is null)
            {
                profile = UserProfile.Create(userId, email, email.Split('@')[0]);
                await userProfileRepository.AddAsync(profile, cancellationToken);
                await userProfileRepository.SaveChangesAsync(cancellationToken);
            }

            cache.Set(cacheKey, profile, CacheDuration);

            return profile;
        }
    }
}
